using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Favalet.Expressions;
using Favalet.Internal;

namespace Favalet.Contexts.Unifiers
{
    [DebuggerStepThrough]
    internal sealed class PlaceholderMarker
    {
        private readonly HashSet<IIdentityTerm> indexes;
#if DEBUG
        private readonly List<IIdentityTerm> list;
#endif
        private PlaceholderMarker(
#if DEBUG
            HashSet<IIdentityTerm> indexes, List<IIdentityTerm> list
#else
            HashSet<IIdentityTerm> indexes
#endif
        )
        {
            this.indexes = indexes;
#if DEBUG
            this.list = list;
#endif
        }

        public bool Mark(IIdentityTerm identity)
        {
#if DEBUG
            list.Add(identity);
#endif
            return indexes.Add(identity);
        }

        public PlaceholderMarker Fork() =>
#if DEBUG
            new PlaceholderMarker(
                new HashSet<IIdentityTerm>(this.indexes),
                new List<IIdentityTerm>(this.list));
#else
            new PlaceholderMarker(
                new HashSet<IIdentityTerm>(this.indexes));
#endif

#if DEBUG
        public override string ToString() =>
            StringUtilities.Join(" ==> ", this.list.Select(index => $"'{index}"));
#endif

        public static PlaceholderMarker Create() =>
#if DEBUG
            new PlaceholderMarker(
                new HashSet<IIdentityTerm>(),
                new List<IIdentityTerm>());
#else
            new PlaceholderMarker(
                new HashSet<IIdentityTerm>());
#endif
    }
}
