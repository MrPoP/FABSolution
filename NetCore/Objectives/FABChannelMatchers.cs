using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public static class FABChannelMatchers
    {
        private static readonly IFABChannelMatcher AllMatcher = new AllChannelMatcher();
        private static readonly IFABChannelMatcher NonServerChannelMatcher = IsNotInstanceOf(typeof(IFABServerChannel));
        private static readonly IFABChannelMatcher ServerChannelMatcher = IsInstanceOf(typeof(IFABServerChannel));
        public static IFABChannelMatcher All()
        {
            return AllMatcher;
        }

        public static IFABChannelMatcher Compose(params IFABChannelMatcher[] matchers)
        {
            if (matchers.Length < 1)
            {
                throw new ArgumentOutOfRangeException("matchers");
            }
            if (matchers.Length == 1)
            {
                return matchers[0];
            }
            return new CompositeMatcher(matchers);
        }

        public static IFABChannelMatcher Invert(IFABChannelMatcher matcher)
        {
            return new InvertMatcher(matcher);
        }

        public static IFABChannelMatcher Is(IFABChannel channel)
        {
            return new InstanceMatcher(channel);
        }

        public static IFABChannelMatcher IsInstanceOf(Type type)
        {
            return new TypeMatcher(type);
        }

        public static IFABChannelMatcher IsNonServerChannel()
        {
            return NonServerChannelMatcher;
        }

        public static IFABChannelMatcher IsNot(IFABChannel channel)
        {
            return Invert(Is(channel));
        }

        public static IFABChannelMatcher IsNotInstanceOf(Type type)
        {
            return Invert(IsInstanceOf(type));
        }

        public static IFABChannelMatcher IsServerChannel()
        {
            return ServerChannelMatcher;
        }

        private sealed class AllChannelMatcher : IFABChannelMatcher
        {
            public bool Matches(IFABChannel channel)
            {
                return true;
            }
        }

        private sealed class CompositeMatcher : IFABChannelMatcher
        {
            private readonly IFABChannelMatcher[] matchers;

            public CompositeMatcher(params IFABChannelMatcher[] matchers)
            {
                this.matchers = matchers;
            }

            public bool Matches(IFABChannel channel)
            {
                IFABChannelMatcher[] matchers = this.matchers;
                for (int i = 0; i < matchers.Length; i++)
                {
                    if (!matchers[i].Matches(channel))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private sealed class InstanceMatcher : IFABChannelMatcher
        {
            private readonly IFABChannel channel;

            public InstanceMatcher(IFABChannel channel)
            {
                this.channel = channel;
            }

            public bool Matches(IFABChannel ch)
            {
                return (this.channel == ch);
            }
        }

        private sealed class InvertMatcher : IFABChannelMatcher
        {
            private readonly IFABChannelMatcher matcher;

            public InvertMatcher(IFABChannelMatcher matcher)
            {
                this.matcher = matcher;
            }

            public bool Matches(IFABChannel channel)
            {
                return !this.matcher.Matches(channel);
            }
        }

        private sealed class TypeMatcher : IFABChannelMatcher
        {
            private readonly Type type;
            public TypeMatcher(Type clazz)
            {
                this.type = clazz;
            }

            public bool Matches(IFABChannel channel)
            {
                return this.type.IsInstanceOfType(channel);
            }
        }
    }
}
