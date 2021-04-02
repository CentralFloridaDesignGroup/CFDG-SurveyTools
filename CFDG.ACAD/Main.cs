﻿using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using CFDG.API;
using ACApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD
{
    public class Commands : IExtensionApplication
    {
        #region Interface Methods

        /// <summary>
        /// Establish the initial event handlers
        /// </summary>
        public void Initialize()
        {
            // Add event handler for every drawing opened.
            ACApplication.DocumentManager.DocumentCreated += LoadDWG;

            // Add event handler for every drawing closed.
            ACApplication.DocumentManager.DocumentDestroyed += UnLoadDWG;

            // Add event handler when AutoCAD goes idle (removes itself after first run to bypass bullshit).
            Autodesk.AutoCAD.ApplicationServices.Application.Idle += OnAppLoad;
        }

        /// <summary>
        /// Fires once when the plugin is unloaded(? assumes when either plugin crashes or application is closed)
        /// </summary>
        public void Terminate()
        {

        }

        #endregion

        #region Internal Methods
        /// <summary>
        /// Runs when the Autocad Application executes Idle event handler. Removes itself after first run.
        /// Litterally a copy paste of LoadDWG() with the added benefit of custom tab addition.
        /// </summary>
        private void OnAppLoad(object s, EventArgs e)
        {
            // Add custom ribbon to RibbonControl
            RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon != null)
            {
                EstablishTab();
                // Remove this event handler as to not fire again.
                // Ensures that the tab is established on startup, but will not create additional.
                Autodesk.AutoCAD.ApplicationServices.Application.Idle -= OnAppLoad;
            }

            ACApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nCFDG Survey plugin version {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} has been loaded successfully\n");

            OnEachDocLoad();
        }

        /// <summary>
        /// Runs when a new drawing is opened.
        /// </summary>
        private void LoadDWG(object s, DocumentCollectionEventArgs e)
        {
            OnEachDocLoad();
        }

        //TODO: Add function to notify user if n amount of documents are open.
        /// <summary>
        /// Shared method between LoadDWG and OnAppLoad
        /// </summary>
        private void OnEachDocLoad()
        {
            if ((bool)XML.ReadValue("Autocad", "EnableOsnapZ"))
            {
                ACApplication.SetSystemVariable("OSnapZ", 1);
            }
        }

        /// <summary>
        /// Runs when a drawing is closed.
        /// </summary>
        private void UnLoadDWG(object s, DocumentDestroyedEventArgs e)
        {
        }

        /// <summary>
        /// Create tab and add panels to tab.
        /// </summary>
        private void EstablishTab()
        {
            //TODO: Fix tab name to include "Survey"
            //Get tab name
            string tabName = (string)XML.ReadValue("General", "CompanyAbbreviation");

            //Add Ribbon
            RibbonControl ribbon = ComponentManager.Ribbon;

            // If the RibbonControl is established, then initialize our tab.
            if (ribbon != null)
            {
                // If the Ribbon already exists, don't create another.
                RibbonTab rtab = ribbon.FindTab("CSurveyTab");
                if (rtab != null)
                {
                    return;
                }

                rtab = new RibbonTab
                {
                    Title = tabName,
                    Id = "CSurveyTab"
                };

                //Project Management Tab
                rtab.Panels.Add(
                    Ribbon.CreatePanel("Project Management", "ProjectManagement",
                    Ribbon.CreateLargeSplitButton(
                        Ribbon.CreateLargeButton("Open\nFolder", "OpenProjectFolder", Properties.Resources.folder),
                        Ribbon.CreateLargeButton("Open Comp\nFolder", "OpenCompFolder", Properties.Resources.folder, Properties.Resources.overlay_edit),
                        Ribbon.CreateLargeButton("Open Field\nData Folder", "OpenFieldDataFold", Properties.Resources.folder, Properties.Resources.overlay_field),
                        Ribbon.CreateLargeButton("Open Submittal\nFolder", "OpenSubmittalFolder", Properties.Resources.folder, Properties.Resources.overlay_export)
                        )
                    )
                );

                //Computations Tab
                rtab.Panels.Add(
                    Ribbon.CreatePanel("Computations", "Computations",
                        Ribbon.CreateLargeButton("Group Comp\nPoints", "GroupPoints", Properties.Resources.Create_PG),
                        Ribbon.CreateLargeButton("Export Point\nGroup", "ExportPointGroup", Properties.Resources.Export_PG),
                        Ribbon.RibbonSpacer,
                        Ribbon.CreateRibbonRow(Ribbon.RibbonRowType.ImageOnly,
                            Ribbon.CreateSmallButton("Slope From Points", "SlopeFromPoints","Calculate slope by selecting two points.", Properties.Resources.SlopeByPoints),
                            Ribbon.CreateSmallButton("Create Measure Down", "Measuredowns", "Create points using slope distance from a reference point.",Properties.Resources.MeasureDown),
                            Ribbon.CreateSmallButton("Footprint", "Footprint","Draw a polyline by entering positive or negative values and angles.",
                            Properties.Resources.footprint)
                        )
                    )
                );

                // Display tab in the RibbonControl for the user.
                if (rtab.Panels.Count != 0)
                {
                    ribbon.Tabs.Add(rtab);
                }
            }

        }
        #endregion
    }
}
