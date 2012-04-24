using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQRSCore;
using System.Runtime.Serialization;

namespace EventStore.FileBased
{
	public class BinaryFileEventStore : IEventStore
	{
		DirectoryInfo dir;
		private readonly IEventPublisher _publisher;
		private readonly List<Type> _knownEventTypes;
		private readonly string _pathToEventStoreFolder;

		public BinaryFileEventStore(IEventPublisher publisher, string pathToEventStoreFolder)
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
			using (var reader = File.Open(Path.Combine(_pathToEventStoreFolder, aggregateId.ToString()), FileMode.Open, FileAccess.ReadWrite))
			{
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
			FileStream writer = File.Open(Path.Combine(_pathToEventStoreFolder, aggregateId.ToString()), FileMode.OpenOrCreate, FileAccess.Read); 
			using (writer)
			{
				foreach (var evt in events)
				{
					var bytes = evt.ToBinary();
					writer.Write(bytes, 0, bytes.Length);
					writer.Flush();
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
