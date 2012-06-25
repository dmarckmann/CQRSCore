using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore
{


	public class Repository<T, TId> where T : AggregateRoot<TId>, new()
	{
		private readonly IEventStore<TId> _storage;

		public Repository(IEventStore<TId> storage)
		{
			_storage = storage;
		}

		public void Save(AggregateRoot<TId> aggregate, int expectedVersion)
		{
			_storage.SaveEvents(aggregate.Id, aggregate.GetUncommittedChanges(), expectedVersion);
		}

		public T GetById(TId id)
		{
			var obj = new T();//lots of ways to do this
			var e = _storage.GetEventsForAggregate(id);
			obj.LoadsFromHistory(e);
			return obj;
		}

	}



  public class Repository<T> : Repository<T, Guid> where T : AggregateRoot<Guid>, new() //shortcut you can do as you see fit with new()
	{
		public Repository(IEventStore<Guid> storage)
			: base(storage)
		{
			
		}
	}


}
