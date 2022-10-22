using OpenKh.Kh2.Events.EventModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2.Events
{
    internal class SerializeHelper
    {
        internal static IEventObject[] FromEventRoots(EventRoot[] array)
        {
            return array
                .Select(
                    it =>
                    {
                        return EventIO.ReadOne(it);
                    }
                )
                .ToArray();
        }

        internal static EventRoot[] ToEventRoots(IEventObject[] objects)
        {
            return objects
                .Select(
                    it => new EventRoot
                    {
                        type = it.GetType().Name,
                        with = it,
                    }
                )
                .ToArray();
        }
    }
}
