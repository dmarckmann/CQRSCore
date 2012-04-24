using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore
{
	public interface IEventPublisher
	{
		void Publish<T>(T @event) where T : Event;
	}
}
