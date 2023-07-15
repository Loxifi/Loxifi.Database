namespace Loxifi.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> GroupByCount<T>(this IEnumerable<T> source, int count)
        {
            IEnumerator<T> enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                throw new ArgumentException("Can not group an empty sequence");
            }

            List<T> list = new(count);

            do
            {
                list.Add(enumerator.Current);

                if (list.Count >= count)
                {
                    yield return list;
                    list = new(count);
                }

                if (!enumerator.MoveNext())
                {
                    if (list.Count > 0)
                    {
                        yield return list;
                    }

                    yield break;
                }
            } while (true);
        }
    }
}
