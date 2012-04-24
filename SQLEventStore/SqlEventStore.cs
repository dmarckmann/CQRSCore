using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace CQRSCore
{
	public class SqlEventStore : IEventStore
	{
		private string _connectionString;
    private readonly IEventPublisher _publisher;
		[Serializable]
		private struct EventDescriptor
		{

			public readonly Event EventData;
			public readonly Guid Id;
			public readonly int Version;

			public EventDescriptor(Guid id, Event eventData, int version)
			{
				EventData = eventData;
				Version = version;
				Id = id;
			}
		}


		public SqlEventStore(IEventPublisher publisher)
			: this(publisher, ConfigurationManager.ConnectionStrings["EventStore"].ConnectionString)
		{
			
		}



	  public SqlEventStore(IEventPublisher publisher, string connectionString) 
		{
			_publisher = publisher;
			_connectionString = connectionString;
		}
		public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
		{
			var lastEvent = GetEventsForAggregate(aggregateId).OrderBy(e => e.Version).LastOrDefault();
			if (lastEvent != null)
			{
				if (lastEvent.Version != expectedVersion && expectedVersion != -1)
				{
					throw new ConcurrencyException();
				}
			}
			var i = expectedVersion;
			foreach (var @event in events)
			{
				i++;
				@event.Version = i;
				SaveEvent(aggregateId, @event, i);

				_publisher.Publish(@event);
			}
		}

		private void SaveEvent(Guid aggregateId, Event @event, int i)
		{
			
			using (SqlConnection Conn = new SqlConnection(_connectionString))
			{
				try
				{
					const string SQL = "INSERT INTO [Events] ([Data],[AggregateId],[Version],[Type]) VALUES (@Data,@Id, @Version, @Type)";
					SqlCommand cmd = new SqlCommand(SQL, Conn);
					cmd.Parameters.AddWithValue("@Id", aggregateId);
					cmd.Parameters.AddWithValue("@Version", i);
					cmd.Parameters.AddWithValue("@Type", @event.GetType().ToString());
					cmd.Parameters.AddWithValue("@Data", ToBinary(new EventDescriptor(aggregateId, @event, i)));
					//					cmd.Parameters.AddWithValue("@DateTimeUploaded", DateTime.Now);

					Conn.Open();
					cmd.ExecuteNonQuery();
					Conn.Close();
				}
				catch
				{
					Conn.Close();
				}
			}
		}

		public List<Event> GetEventsForAggregate(Guid aggregateId)
		{


			List<EventDescriptor> eventDescriptors = new List<EventDescriptor>();
			using (SqlConnection Conn = new SqlConnection(_connectionString))
			{
				try
				{
					const string sql = "SELECT [Data],[AggregateId],[Version],[Type] FROM [Events] WHERE AggregateId = @Id";
					SqlCommand cmd = new SqlCommand(sql, Conn);
					cmd.Parameters.AddWithValue("@Id", aggregateId);

					Conn.Open();
					var reader = cmd.ExecuteReader();
					if (!reader.HasRows)
					{
						throw new AggregateNotFoundException();
					}
					while (reader.Read())
					{
						int version = reader.GetInt32(reader.GetOrdinal("Version"));
						eventDescriptors.Add(FromBinary((byte[])reader["Data"]));
					}
					Conn.Close();
				}
				catch
				{
					Conn.Close();
				}

				return eventDescriptors.Select(desc => desc.EventData).ToList();
			}
		}

		public List<Guid> GetAggregateRootIds()
		{
			List<Guid> ids = new List<Guid>();
			using (SqlConnection Conn = new SqlConnection(_connectionString))
			{
				try
				{
					const string sql = "SELECT DISTINCT [AggregateId] FROM [Events]";
					SqlCommand cmd = new SqlCommand(sql, Conn);
					
					Conn.Open();
					var reader = cmd.ExecuteReader();
					if (!reader.HasRows)
					{
						throw new AggregateNotFoundException();
					}
					while (reader.Read())
					{
						ids.Add(reader.GetGuid(reader.GetOrdinal("AggregateId")));
					}
					Conn.Close();
				}
				catch
				{
					Conn.Close();
				}

				return ids;
			}
		}



		private static byte[] ToBinary(EventDescriptor document)
		{
			using (var memoryStream = new MemoryStream())
			{
				new BinaryFormatter().Serialize(memoryStream, document);
				return memoryStream.ToArray();
			}
		}

		private static EventDescriptor FromBinary(byte[] data)
		{
			using (var memoryStream = new MemoryStream(data))
			{
				return (EventDescriptor)new BinaryFormatter().Deserialize(memoryStream);
			}
		}


	
	}
}
