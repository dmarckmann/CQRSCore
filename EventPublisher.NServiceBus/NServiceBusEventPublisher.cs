using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQRSCore;
using NServiceBus;
using NServiceBus.Config;

using System.Reflection;


namespace EventPublisher.NServiceBus
{
	public class NServiceBusEventPublisher : IEventPublisher
	{

		IBus _bus;

		public NServiceBusEventPublisher(params Assembly[] assemblies)
		{
			_bus = Configure.With(assemblies)
			.DefaultBuilder()
			.XmlSerializer()
			.MsmqTransport()
				.IsTransactional(true)
			.UnicastBus()
			.MsmqSubscriptionStorage()
			.CreateBus()
				.Start();
		}

		public void Publish<T>(T @event) where T : Event
		{
			//In order to use NServiceBus your events have to implement IMessage
			if (!(@event is IMessage))
				throw new ArgumentException("The event must be of type IMessage. In order to use NServiceBus your events have to implement IMessage.");
			IMessage msg = @event as IMessage;
			if(msg != null)
				_bus.Publish(msg);
		}
	}
}
