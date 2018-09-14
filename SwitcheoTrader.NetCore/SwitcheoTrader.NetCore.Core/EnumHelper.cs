﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcheoTrader.NetCore.Core
{
    public static class EnumHelper
    {
        /// <summary>
        /// Get Description form an enum
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>string of description</returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
