using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore
{
	public interface IEventStore<TId>
	{
		void SaveEvents(TId aggregateId, IEnumerable<Event> events, int expectedVersion);
		List<Event> GetEventsForAggregate(TId aggregateId);
		List<TId> GetAggregateRootIds();
	}

  public interface IEventStore : IEventStore<Guid>
	{
	}
}
