using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore
{
	public interface IRepository<T, TId> where T : AggregateRoot, new()
	{
		void Save(AggregateRoot aggregate, int expectedVersion);
		T GetById(TId id);
	}
}
