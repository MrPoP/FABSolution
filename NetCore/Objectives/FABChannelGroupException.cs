using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABChannelGroupException : FABChannelException, IEnumerable<KeyValuePair<IFABChannel, Exception>>, IEnumerable
    {
        private readonly IReadOnlyCollection<KeyValuePair<IFABChannel, Exception>> failed;

        public FABChannelGroupException(IList<KeyValuePair<IFABChannel, Exception>> exceptions)
        {
            if (exceptions == null)
            {
                throw new ArgumentNullException("exceptions");
            }
            if (exceptions.Count == 0)
            {
                throw new ArgumentException("excetpions must be not empty.");
            }
            this.failed = new ReadOnlyCollection<KeyValuePair<IFABChannel, Exception>>(exceptions);
        }

        public IEnumerator<KeyValuePair<IFABChannel, Exception>> GetEnumerator()
        {
            return this.failed.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.failed.GetEnumerator();
        }
    }
}
