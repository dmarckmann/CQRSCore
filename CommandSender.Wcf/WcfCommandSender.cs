using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQRSCore;
using System.ServiceModel;

namespace CommandSender.Wcf
{
	[ServiceContract]
	public interface IWcfCommandSender : ICommandSender
	{
		[OperationContract]
		void Send(Command command);
	}

	public class WcfCommandSender : ICommandSender, IWcfCommandSender
	{
		private readonly Dictionary<Type, List<Action<Message>>> _routes = new Dictionary<Type, List<Action<Message>>>();

		public void RegisterHandler<T>(Action<T> handler) where T : Message
		{
			List<Action<Message>> handlers;
			if (!_routes.TryGetValue(typeof(T), out handlers))
			{
				handlers = new List<Action<Message>>();
				_routes.Add(typeof(T), handlers);
			}
			handlers.Add(DelegateAdjuster.CastArgument<Message, T>(x => handler(x)));
		}

		public void Send<T>(T command) where T : Command
		{
			List<Action<Message>> handlers;
			if (_routes.TryGetValue(typeof(T), out handlers))
			{
				if (handlers.Count != 1) throw new InvalidOperationException("cannot send to more than one handler");
				handlers[0](command);
			}
			else
			{
				throw new InvalidOperationException("no handler registered");
			}
		}

		public void Send(Command command)
		{
			Send<Command>(command);
		}
	}
}
