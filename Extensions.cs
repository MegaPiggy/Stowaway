using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stowaway
{
	public static class Extensions
	{
		/// <summary>
		/// Returns if a value in the <paramref name="source"/> meets both the <paramref name="condition"/> and <paramref name="secondCondition"/>.
		/// 
		/// If none of them do then it returns the first value that meets the <paramref name="secondCondition"/>.
		/// </summary>
		public static TSource FirstSecondOrDefault<TSource>(this IEnumerable<TSource> source, Predicate<TSource> condition, Predicate<TSource> secondCondition, TSource customDefault = default(TSource))
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (secondCondition == null)
				throw new ArgumentNullException(nameof(secondCondition));
			foreach (TSource source1 in source)
			{
				if (condition(source1) && secondCondition(source1))
					return source1;
			}
			foreach (TSource source1 in source)
			{
				if (secondCondition(source1))
					return source1;
			}
			return customDefault;
		}

		/// <summary>
		/// Returns if a value in the <paramref name="source"/> meets the <paramref name="condition"/>.
		/// 
		/// If none of them do then it returns the first value.
		/// </summary>
		public static TSource FirstSecondOrDefault<TSource>(this IEnumerable<TSource> source, Predicate<TSource> condition)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			foreach (TSource source1 in source)
			{
				if (condition(source1))
					return source1;
			}
			return source.FirstOrDefault();
		}
	}
}
