using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore
{
	public interface IRepository<T, TId> where T : AggregateRoot<TId>, new()
	{
		void Save(AggregateRoot<TId> aggregate, int expectedVersion);
		T GetById(TId id);
	}
}
