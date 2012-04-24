using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CQRSCore;

namespace CQRSCore.Testing
{
	public abstract class AggregateRootTest<T> where T :AggregateRoot,  new()
	{
		public AggregateRootTest()
		{
			try
			{
				Tested = new T();
				IEnumerable<Event> events = Given();
				Tested.LoadsFromHistory(events);
				When();
				UnCommittedEvents = Tested.GetUncommittedChanges();
			}
			catch (Exception ex)
			{
				Caught = ex;
			}
		}

		public abstract IEnumerable<Event> Given();
		public abstract void When();

		public T Tested { get; set; }
		public Exception Caught { get; set; }
		public IEnumerable<Event> UnCommittedEvents { get; set; }
	}
}
