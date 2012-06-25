using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore
{
	public class AggregateRoot<T> 
	{

		public T Id { get; private set; }

		public int Version { get; set; }

		private readonly List<Event> _changes = new List<Event>();


		public IEnumerable<Event> GetUncommittedChanges()
		{
			return _changes;
		}

		public void MarkChangesAsCommitted()
		{
			_changes.Clear();
		}

		public void LoadsFromHistory(IEnumerable<Event> history)
		{
			foreach (var e in history) ApplyChange(e, false);
		}

		protected void ApplyChange(Event @event)
		{
			ApplyChange(@event, true);
		}

		private void ApplyChange(Event @event, bool isNew)
		{
			this.AsDynamic().Apply(@event);
			this.Version = @event.Version;
			if (isNew) _changes.Add(@event);
		}
	}

  public abstract class AggregateRoot : AggregateRoot<Guid>
	{
		
	}
}