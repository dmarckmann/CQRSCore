using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQRSCore;
using RabbitMQ.Client;

namespace RabbitMQPublisher
{
	public class RabbitMQEventPublisher : IEventPublisher
	{
		private readonly string _exchange;
		private readonly string _hostName;
		private readonly string _routingKey;
    public RabbitMQEventPublisher() : this("localhost", "events", "")
		{		}

		public RabbitMQEventPublisher(string hostName, string exchange, string routingKey)
		{
			_routingKey = routingKey;
      _hostName = hostName;
      _exchange = exchange;
		}

		public void Publish<T>(T @event) where T : Event
		{
			ConnectionFactory factory = new ConnectionFactory();
			factory.HostName = _hostName;
			using (IConnection connection = factory.CreateConnection())
			using (IModel channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(_exchange, "fanout");
				byte[] body = @event.ToBinary();
				channel.BasicPublish(_exchange, _routingKey, null, body);
			}
		}
	}
}
