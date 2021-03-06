// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using global::System;
using global::System.Diagnostics;
using global::System.Reflection.Runtime.TypeInfos;

using global::Internal.Reflection.Core.NonPortable;

namespace System.Reflection.Runtime.Dispensers
{
    //
    // Creates the appropriate Dispenser for a scenario, based on the dispenser policy.
    //
    internal static class DispenserFactory
    {
        //
        // Note: If your K is a valuetype, use CreateDispenserV() instead. Some algorithms will not be available for use.
        //
        public static Dispenser<K, V> CreateDispenser<K, V>(DispenserScenario scenario, Func<K, V> factory)
            where K : class, IEquatable<K>
            where V : class
        {
            DispenserAlgorithm algorithm = _dispenserPolicy.GetAlgorithm(scenario);
            if (algorithm == DispenserAlgorithm.ReuseAsLongAsKeyIsAlive)
                return new DispenserThatReusesAsLongAsKeyIsAlive<K, V>(factory);
            else
                return CreateDispenserV<K, V>(scenario, factory);

            throw new Exception();
        }


        //
        // This is similar to CreateDispenser() except it doesn't constrain the key to be a reference type.
        // As a result, some algorithms will not be available for use.
        //
        public static Dispenser<K, V> CreateDispenserV<K, V>(DispenserScenario scenario, Func<K, V> factory)
            where K : IEquatable<K>
            where V : class
        {
            DispenserAlgorithm algorithm = _dispenserPolicy.GetAlgorithm(scenario);

            Debug.Assert(algorithm != DispenserAlgorithm.ReuseAsLongAsKeyIsAlive,
                "Use CreateDispenser() if you want to use this algorithm. The key must not be a valuetype.");

            if (algorithm == DispenserAlgorithm.CreateAlways)
                return new DispenserThatAlwaysCreates<K, V>(factory);
            else if (algorithm == DispenserAlgorithm.ReuseAlways)
                return new DispenserThatAlwaysReuses<K, V>(factory);
            else if (algorithm == DispenserAlgorithm.ReuseAsLongAsValueIsAlive)
                return new DispenserThatReusesAsLongAsValueIsAlive<K, V>(factory);
            else if (algorithm == DispenserAlgorithm.LatchesTypeInfoInsideType)
                return (Dispenser<K, V>)(Object)(new DispenserThatLatchesTypeInfosInsideTypes((Func<RuntimeType, RuntimeTypeInfo>)(Object)factory));

            throw new Exception();
        }


        private static DispenserPolicy _dispenserPolicy = new DefaultDispenserPolicy();
    }
}


