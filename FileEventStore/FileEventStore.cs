using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQRSCore;
using System.Runtime.Serialization;

namespace EventStore.FileBased
{
	public class FileEventStore : IEventStore
	{
		DirectoryInfo dir;
		private readonly IEventPublisher _publisher;
		private readonly List<Type> _knownEventTypes;
		private readonly string _pathToEventStoreFolder;
  
		public FileEventStore(IEventPublisher publisher, string pathToEventStoreFolder, List<Type> knownEventTypes)
		{
			_pathToEventStoreFolder = pathToEventStoreFolder;
      _knownEventTypes = knownEventTypes;
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
				var serializer = new DataContractSerializer(typeof(Event), _knownEventTypes);
				using (var stream = new MemoryStream())
				{
					var arr = File.ReadAllLines(Path.Combine(_pathToEventStoreFolder, aggregateId.ToString()));
					byte[] uTF8EncodingDefaultGetBytes = UTF8Encoding.Default.GetBytes(arr[0].ToCharArray());
					stream.Write(uTF8EncodingDefaultGetBytes, 0, uTF8EncodingDefaultGetBytes.Length);
					stream.Position = 0;
					list.Add((Event)serializer.ReadObject(stream));
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
				foreach (var evt in events)
				{
					var serializer = new DataContractSerializer(typeof(Event), _knownEventTypes);
					using (var stream = new MemoryStream())
					{
						serializer.WriteObject(stream, evt);
						writer.WriteLine(UTF8Encoding.Default.GetString(stream.ToArray(), 0, (int)stream.Length));
						writer.Flush();
					}
					_publisher.Publish(evt);
				}
			}

		}

		public List<Guid> GetAggregateRootIds()
		{
			return Directory.GetFiles(_pathToEventStoreFolder).Select(i => Guid.Parse(i.Substring(i.LastIndexOf('\\') + 1))).ToList();
		}
	}
}
