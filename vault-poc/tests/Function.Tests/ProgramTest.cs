using System;
using Function;
using NUnit.Framework;

namespace Function.Tests {
	public class GreeterTest {
		[Test]
		public void ResourcePricipalThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Vault());
		}
	}
}
