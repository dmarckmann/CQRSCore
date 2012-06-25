using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQRSCore;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EventStore.FileBased
{
	public class FileEventStore : IEventStore<Guid>
	{
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

		DirectoryInfo dir;
		private readonly IEventPublisher _publisher;
		private readonly string _pathToEventStoreFolder;
  
		public FileEventStore(IEventPublisher publisher, string pathToEventStoreFolder)
		{
			_pathToEventStoreFolder = pathToEventStoreFolder;
      _publisher = publisher;
			dir = new DirectoryInfo(_pathToEventStoreFolder); 
			Console.WriteLine(dir.FullName);
			if (!Directory.Exists(dir.FullName))
			{
				Directory.CreateDirectory(dir.FullName);
			}
		}

		public List<Event> GetEventsForAggregate(Guid aggregateId)
		{
			List<Event> list = new List<Event>();
			using (var reader = File.OpenRead(Path.Combine(_pathToEventStoreFolder, aggregateId.ToString())))
			{
				using (var stream = new MemoryStream())
				{
					var arr = File.ReadAllLines(Path.Combine(_pathToEventStoreFolder, aggregateId.ToString()));
					byte[] uTF8EncodingDefaultGetBytes = UTF8Encoding.Default.GetBytes(arr[0].ToCharArray());
					var descriptor = FromBinary(uTF8EncodingDefaultGetBytes);
					list.Add(descriptor.EventData);
				}
			}
			return list;
		}

		public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
		{
			StreamWriter writer = null;
			if (!File.Exists(Path.Combine(_pathToEventStoreFolder, aggregateId.ToString()) ))
				writer = File.CreateText(Path.Combine(_pathToEventStoreFolder, aggregateId.ToString()));
			else
			  writer = File.AppendText(Path.Combine(_pathToEventStoreFolder, aggregateId.ToString()));
			using (writer)
			{
				var i = expectedVersion;
				foreach (var evt in events)
				{
					i++;
					evt.Version = i;
					SaveEvent(writer, aggregateId, evt, i);
					_publisher.Publish(evt);
				}
			}

		}

		private void SaveEvent(StreamWriter writer, Guid aggregateId, Event @event, int i)
		{
				writer.WriteLine(ToBinary(new EventDescriptor(aggregateId, @event, i)));
				writer.Flush();
		}
		public List<Guid> GetAggregateRootIds()
		{
			return Directory.GetFiles(_pathToEventStoreFolder).Select(i => Guid.Parse(i.Substring(i.LastIndexOf('\\') + 1))).ToList();
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
