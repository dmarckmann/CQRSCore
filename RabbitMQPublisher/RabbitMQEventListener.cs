using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQRSCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace RabbitMQPublisher
{
	public class RabbitMQEventListener
	{
		private readonly string _exchange;
		private readonly string _hostName;
    public RabbitMQEventListener() : this("localhost", "events", "")
		{		}

		public RabbitMQEventListener(string hostName, string exchange, string routingKey)
		{
			_routingKey = routingKey;
      _hostName = hostName;
      _exchange = exchange;
		}

		private readonly Dictionary<Type, List<Action<Event>>> _routes = new Dictionary<Type, List<Action<Event>>>();
		private readonly string _routingKey;
  
		public void RegisterHandler<T>(Action<T> handler) where T : Event
		{
			List<Action<Event>> handlers;
			if (!_routes.TryGetValue(typeof(T), out handlers))
			{
				handlers = new List<Action<Event>>();
				_routes.Add(typeof(T), handlers);
			}
			handlers.Add(DelegateAdjuster.CastArgument<Event, T>(x => handler(x)));
		}

		public void Listen()
		{
			ConnectionFactory factory = new ConnectionFactory();
			factory.HostName = _hostName;
			using (IConnection connection = factory.CreateConnection())
			using (IModel channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(_exchange, "fanout");
				string queue_name = channel.QueueDeclare();
				channel.QueueBind(queue_name, _exchange, _routingKey); 
				QueueingBasicConsumer consumer = new QueueingBasicConsumer(channel); 
				channel.BasicConsume(queue_name, true, consumer);
				while(true) {
					BasicDeliverEventArgs ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
					byte[] body = ea.Body;
					
					Event evt = Event.FromBinary(body);
					Handle(evt);
			}
		}
		}

		private void Handle(Event @event)
		{
			Console.WriteLine("Handling event: {0}", @event.GetType().ToString());
			List<Action<Event>> handlers;
			if (!_routes.TryGetValue(@event.GetType(), out handlers)) 
				return;
			foreach (var handler in handlers)
			{
				//dispatch on thread pool for added awesomeness
				var handler1 = handler;
				ThreadPool.QueueUserWorkItem(x => handler1(@event));
			}
		}
	}
}
