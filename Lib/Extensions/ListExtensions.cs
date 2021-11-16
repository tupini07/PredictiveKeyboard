namespace Lib.Extensions
{
    internal static class ListExtensions
    {
        public static string GetListId<T>(this List<T> list)
        {
            return string.Join("", list);
        }


        public static T PopLeft<T>(this List<T> list)
        {
            var element = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                list[i - 1] = list[i];
            }

            list.RemoveAt(list.Count - 1);
            return element;
        }

        public static List<T> CopyList<T>(this List<T> list)
        {
            var newList = new List<T>();
            foreach (var item in list)
            {
                newList.Add(item);
            }
            return newList;
        }
    }
}
