﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine;

namespace BeatSaberPlus.Plugins.SongChartVisualizer.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal class Settings : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIComponent("followenvironementrotations-toggle")]
        public ToggleSetting m_FollowEnvironementRotations;
        [UIComponent("backgroundopacity-increment")]
        public IncrementSetting m_BackgroundOpacity;
        [UIComponent("cursoropacity-increment")]
        private IncrementSetting m_CursorOpacity;
        [UIComponent("lineopacity-increment")]
        private IncrementSetting m_LineOpacity;
        [UIComponent("legendopacity-increment")]
        private IncrementSetting m_LegendOpacity;
        [UIComponent("dashopacity-increment")]
        private IncrementSetting m_DashOpacity;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("shownpslegend-toggle")]
        private ToggleSetting m_ShowNPSLegend;
        [UIComponent("background-color")]
        private ColorSetting m_BackgroundColor;
        [UIComponent("cursor-color")]
        private ColorSetting m_CursorColor;
        [UIComponent("line-color")]
        private ColorSetting m_LineColor;
        [UIComponent("legend-color")]
        private ColorSetting m_LegendColor;
        [UIComponent("dash-color")]
        private ColorSetting m_DashColor;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;

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
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// If first activation, bind event
            if(p_FirstActivation)
            {
                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                /// Left
                Utils.GameUI.PrepareToggleSetting(m_FollowEnvironementRotations, l_Event,                                    Config.SongChartVisualizer.FollowEnvironementRotation,   true);
                Utils.GameUI.PrepareIncrementSetting(m_BackgroundOpacity,        l_Event, Utils.GameUI.Formatter_Percentage, Config.SongChartVisualizer.BackgroundA,                  true);
                Utils.GameUI.PrepareIncrementSetting(m_CursorOpacity,            l_Event, Utils.GameUI.Formatter_Percentage, Config.SongChartVisualizer.CursorA,                      true);
                Utils.GameUI.PrepareIncrementSetting(m_LineOpacity,              l_Event, Utils.GameUI.Formatter_Percentage, Config.SongChartVisualizer.LineA,                        true);
                Utils.GameUI.PrepareIncrementSetting(m_LegendOpacity,            l_Event, Utils.GameUI.Formatter_Percentage, Config.SongChartVisualizer.LegendA,                      true);
                Utils.GameUI.PrepareIncrementSetting(m_DashOpacity,              l_Event, Utils.GameUI.Formatter_Percentage, Config.SongChartVisualizer.DashLineA,                    true);

                /// Right
                Utils.GameUI.PrepareToggleSetting(m_ShowNPSLegend,  l_Event, Config.SongChartVisualizer.ShowNPSLegend,      true);
                Utils.GameUI.PrepareColorSetting(m_BackgroundColor, l_Event, Config.SongChartVisualizer.BackgroundColor,    true);
                Utils.GameUI.PrepareColorSetting(m_CursorColor,     l_Event, Config.SongChartVisualizer.CursorColor,        true);
                Utils.GameUI.PrepareColorSetting(m_LineColor,       l_Event, Config.SongChartVisualizer.LineColor,          true);
                Utils.GameUI.PrepareColorSetting(m_LegendColor,     l_Event, Config.SongChartVisualizer.LegendColor,        true);
                Utils.GameUI.PrepareColorSetting(m_DashColor,       l_Event, Config.SongChartVisualizer.DashLineColor,      true);
            }

            SongChartVisualizer.Instance.RefreshPreview();
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            /// Forward event
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            SongChartVisualizer.Instance.DestroyPreview();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            /// Update config
            Config.SongChartVisualizer.FollowEnvironementRotation   = m_FollowEnvironementRotations.Value;
            Config.SongChartVisualizer.ShowNPSLegend                = m_ShowNPSLegend.Value;
            Config.SongChartVisualizer.BackgroundA                  = m_BackgroundOpacity.Value;
            Config.SongChartVisualizer.BackgroundR                  = m_BackgroundColor.CurrentColor.r;
            Config.SongChartVisualizer.BackgroundG                  = m_BackgroundColor.CurrentColor.g;
            Config.SongChartVisualizer.BackgroundB                  = m_BackgroundColor.CurrentColor.b;
            Config.SongChartVisualizer.CursorA                      = m_CursorOpacity.Value;
            Config.SongChartVisualizer.CursorR                      = m_CursorColor.CurrentColor.r;
            Config.SongChartVisualizer.CursorG                      = m_CursorColor.CurrentColor.g;
            Config.SongChartVisualizer.CursorB                      = m_CursorColor.CurrentColor.b;
            Config.SongChartVisualizer.LineA                        = m_LineOpacity.Value;
            Config.SongChartVisualizer.LineR                        = m_LineColor.CurrentColor.r;
            Config.SongChartVisualizer.LineG                        = m_LineColor.CurrentColor.g;
            Config.SongChartVisualizer.LineB                        = m_LineColor.CurrentColor.b;
            Config.SongChartVisualizer.LegendA                      = m_LegendOpacity.Value;
            Config.SongChartVisualizer.LegendR                      = m_LegendColor.CurrentColor.r;
            Config.SongChartVisualizer.LegendG                      = m_LegendColor.CurrentColor.g;
            Config.SongChartVisualizer.LegendB                      = m_LegendColor.CurrentColor.b;
            Config.SongChartVisualizer.DashLineA                    = m_DashOpacity.Value;
            Config.SongChartVisualizer.DashLineR                    = m_DashColor.CurrentColor.r;
            Config.SongChartVisualizer.DashLineG                    = m_DashColor.CurrentColor.g;
            Config.SongChartVisualizer.DashLineB                    = m_DashColor.CurrentColor.b;

            /// Refresh preview
            SongChartVisualizer.Instance.RefreshPreview();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void OnResetButton()
        {
            m_PreventChanges = true;

            /// Set values
            m_FollowEnvironementRotations.Value = Config.SongChartVisualizer.FollowEnvironementRotation;
            m_BackgroundOpacity.Value           = Config.SongChartVisualizer.BackgroundA;
            m_CursorOpacity.Value               = Config.SongChartVisualizer.CursorA;
            m_LineOpacity.Value                 = Config.SongChartVisualizer.LineA;
            m_LegendOpacity.Value               = Config.SongChartVisualizer.LegendA;
            m_DashOpacity.Value                 = Config.SongChartVisualizer.DashLineA;

            /// Set values
            m_ShowNPSLegend.Value           = Config.SongChartVisualizer.ShowNPSLegend;
            m_BackgroundColor.CurrentColor  = new Color(Config.SongChartVisualizer.BackgroundR, Config.SongChartVisualizer.BackgroundG, Config.SongChartVisualizer.BackgroundB, 1f);
            m_CursorColor.CurrentColor      = new Color(Config.SongChartVisualizer.CursorR,     Config.SongChartVisualizer.CursorG,     Config.SongChartVisualizer.CursorB,     1f);
            m_LineColor.CurrentColor        = new Color(Config.SongChartVisualizer.LineR,       Config.SongChartVisualizer.LineG,       Config.SongChartVisualizer.LineB,       1f);
            m_LegendColor.CurrentColor      = new Color(Config.SongChartVisualizer.LegendR,     Config.SongChartVisualizer.LegendG,     Config.SongChartVisualizer.LegendB,     1f);
            m_DashColor.CurrentColor        = new Color(Config.SongChartVisualizer.DashLineR,   Config.SongChartVisualizer.DashLineG,   Config.SongChartVisualizer.DashLineB,   1f);

            m_PreventChanges = false;

            SongChartVisualizer.Instance.RefreshPreview();
        }
    }
}
