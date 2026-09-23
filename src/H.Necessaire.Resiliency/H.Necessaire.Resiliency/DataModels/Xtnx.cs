using H.Necessaire.Resiliency.Abstractions.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace H.Necessaire.Resiliency.DataModels
{
    public static class Xtnx
    {
        /// <summary>
        /// Self has priority, therefore ID is kept from self and conflict resolution default to self unless otherwise specified
        /// </summary>
        /// <param name="self">Self context</param>
        /// <param name="addOnContexts">Context to merge data from</param>
        /// <param name="conflictResolver">Custom conflict resolver, optional, defaults to self</param>
        /// <returns>New context instance with data from all contexts</returns>
        public static ImAnHResiliencyMeasurementContext MergeWith(this ImAnHResiliencyMeasurementContext self, IEnumerable<IReadOnlyDictionary<string, object>> addOnContexts, Func<string, object, IEnumerable<object>, object> conflictResolver = null)
        {
            IEnumerable<IReadOnlyDictionary<string, object>> all
                = (self is null ? Enumerable.Empty<IReadOnlyDictionary<string, object>>() : self.AsArray())
                .Concat(addOnContexts ?? Enumerable.Empty<IReadOnlyDictionary<string, object>>())
                .Where(x => x != null && x.Count > 0)
                ;

            conflictResolver = conflictResolver ?? ResolveDefaultContextKeyConflict;

            Dictionary<string, object> merged
                = all
                .SelectMany(x => x)
                .GroupBy(x => x.Key)
                .ToDictionary(
                    keySelector: group => group.Key,
                    elementSelector: group =>
                    {
                        object[] options = group.Select(x => x.Value).ToArray();
                        if (options.Length == 1)
                            return options[0];
                        return conflictResolver(group.Key, options[0], options.Skip(1));
                    }
                );

            return
                ((HResiliencyMeasurementContext)merged)
                .And(ctx =>
                {
                    ctx.ID = self?.ID;
                    ctx.Notes = self?.Notes;
                });
        }

        public static ImAnHResiliencyMeasurementContext MergeWith(this ImAnHResiliencyMeasurementContext self, params IReadOnlyDictionary<string, object>[] addOnContexts)
            => self.MergeWith(addOnContexts, conflictResolver: null);

        public static ImAnHResiliencyMeasurementContext MergeWith(this ImAnHResiliencyMeasurementContext self, IEnumerable<ImAnHResiliencyMeasurementContext> addOnContexts, Func<string, object, IEnumerable<KeyValuePair<string, object>>, object> conflictResolver = null)
        {
            IEnumerable<ImAnHResiliencyMeasurementContext> all
                = (self is null ? Enumerable.Empty<ImAnHResiliencyMeasurementContext>() : self.AsArray())
                .Concat(addOnContexts ?? Enumerable.Empty<ImAnHResiliencyMeasurementContext>())
                .Where(x => x != null && x.Count > 0)
                ;

            conflictResolver = conflictResolver ?? ResolveDefaultContextKeyConflict;

            string[] allKeys = all.SelectMany(x => x.Keys).Distinct().ToArray();
            if (allKeys.IsEmpty())
                return new HResiliencyMeasurementContext().And(ctx =>
                {
                    ctx.ID = self?.ID;
                    ctx.Notes = self?.Notes;
                });

            var merged
                = allKeys
                .ToDictionary(
                    keySelector: key => key,
                    elementSelector: key =>
                    {
                        KeyValuePair<string, object>[] options = all.Select(c => !c.TryGetValue(key, out var value) ? (null as KeyValuePair<string, object>?) : new KeyValuePair<string, object>(c.ID, value)).Where(x => x != null).Select(x => x.Value).ToArray();
                        if (options.Length == 1)
                            return options[0];
                        return conflictResolver(key, options[0].Value, options.Skip(1));
                    }
                );

            return
                ((HResiliencyMeasurementContext)merged)
                .And(ctx =>
                {
                    ctx.ID = self?.ID;
                    ctx.Notes = all.SelectMany(x => x.Notes ?? Array.Empty<Note>()).Where(n => !n.IsEmpty()).ToArrayNullIfEmpty();
                });
        }

        public static ImAnHResiliencyMeasurementContext MergeWith(this ImAnHResiliencyMeasurementContext self, params ImAnHResiliencyMeasurementContext[] addOnContexts)
            => self.MergeWith(addOnContexts, conflictResolver: null);

        static object ResolveDefaultContextKeyConflict(string key, object self, IEnumerable<object> addOns) => self;
        static object ResolveDefaultContextKeyConflict(string key, object self, IEnumerable<KeyValuePair<string, object>> addOns) => self;
    }
}
