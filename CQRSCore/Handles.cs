using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore
{
	public interface Handles<T>
	{
		void Handle(T message);
	}
}
