﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Common.Infrastructure.Cores.MockCore
{
    [ExcludeFromCodeCoverage]
	public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
	{
		private readonly IEnumerator<T> _enumerator;

		public TestAsyncEnumerator(IEnumerator<T> enumerator)
		{
			_enumerator = enumerator ?? throw new ArgumentNullException();
		}

		public T Current => _enumerator.Current;

		public ValueTask DisposeAsync()
		{
			_enumerator.Dispose();
			return new ValueTask();
		}

		public ValueTask<bool> MoveNextAsync()
		{
			return new ValueTask<bool>(_enumerator.MoveNext());
		}
	}
}
