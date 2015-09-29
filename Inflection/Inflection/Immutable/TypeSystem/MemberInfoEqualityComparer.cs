namespace Inflection.Immutable.TypeSystem
{
    using System.Collections.Generic;
    using System.Reflection;

    public class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>
    {
        public static readonly MemberInfoEqualityComparer Default = new MemberInfoEqualityComparer();

        public bool Equals(MemberInfo x, MemberInfo y)
        {
            if (x == y)
            {
                return true;
            }

            if (x.DeclaringType != y.DeclaringType)
            {
                return false;
            }

            if (x.Name != y.Name)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(MemberInfo obj)
        {
            if (obj == null)
            {
                return 0;
            }

            unchecked
            {
                var seed = 0;

                if (obj.DeclaringType != null)
                {
                    seed ^= obj.DeclaringType.GetHashCode() * 397;
                }

                return seed ^ obj.Name.GetHashCode();
            }
        }
    }
}