﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using UnityEngine;

namespace BeatSaberPlus.Plugins.MenuMusic.UI
{
    /// <summary>
    /// Menu music settings credit view
    /// </summary>
    internal class SettingsCredits : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("CreditBackground")]
        internal GameObject CreditBackground = null;
        [UIValue("Line1")]
        private readonly string m_Line1 = "<u><b>Thanks to <b>Lunikc</b> for original idea</b></u>";
        [UIValue("Line2")]
        private readonly string m_Line2 = " ";
        [UIValue("Line3")]
        private readonly string m_Line3 = " ";
        [UIValue("Line4")]
        private readonly string m_Line4 = " ";
        [UIValue("Line5")]
        private readonly string m_Line5 = " ";
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            if (p_FirstActivation)
            {
                /// Update background color
                var l_Color = CreditBackground.GetComponent<ImageView>().color;
                l_Color.a = 0.5f;

                CreditBackground.GetComponent<ImageView>().color = l_Color;
            }
        }
    }
}
