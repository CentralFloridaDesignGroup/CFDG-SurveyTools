using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CFDG.API;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD
{
    public class UserInput
    {
        #region Private Properties

        public static bool RequireZenethCheck { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get string from user in AutoCAD.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetStringFromUser(string message)
        {
            (_, Editor AcEditor) = GetCurrentDocSpace();

            var pso = new PromptStringOptions(message)
            {
                AllowSpaces = false
            };

            PromptResult tr = AcEditor.GetString(pso);
            if (tr.Status == PromptStatus.Cancel)
            {
                return "";
            }
            return tr.StringResult;
        }

        /// <summary>
        /// Select a point in the current document (3D)
        /// </summary>
        /// <param name="message">Message for the prompt</param>
        /// <returns>A 3D point</returns>
        public static Point3d SelectPointInDoc(string message)
        {
            return SelectPointInDoc(message, new Point3d(-1, -1, -1));
        }

        /// <summary>
        /// Select a point in the current document (3D)
        /// </summary>
        /// <param name="message">Message for the prompt</param>
        /// <param name="basePoint">A base point for reference</param>
        /// <returns>A 3D point</returns>
        public static Point3d SelectPointInDoc(string message, Point3d basePoint)
        {
            (_, Editor AcEditor) = GetCurrentDocSpace();

            if (VerifyZenthValues(true))
            {
                var ppo = new PromptPointOptions($"\n{message}")
                {
                    AllowArbitraryInput = true,
                    AllowNone = false
                };
                if (basePoint != new Point3d(-1, -1, -1))
                {
                    ppo.BasePoint = basePoint;
                    ppo.UseBasePoint = true;
                    ppo.UseDashedLine = true;
                }

                PromptPointResult pr = AcEditor.GetPoint(ppo);
                if (pr.Status != PromptStatus.Cancel)
                {
                    return pr.Value;
                }
                return new Point3d(-1, -1, -1);
            }
            return new Point3d(-1, -1, -1);
        }


        /// <summary>
        /// Select a angle in the current document
        /// </summary>
        /// <param name="message">Message for the prompt</param>
        /// <param name="basePoint">A base point for reference</param>
        /// <param name="distance">The distance to break apart.</param>
        /// <returns>A 2D Vector</returns>
        public static Vector2d SelectAngleInDoc(string message, Point3d basePoint, double distance)
        {
            var angle = SelectAngleInDoc(message, basePoint);

            var measures = new Triangle(distance, angle);

            return new Vector2d(measures.SideA, measures.SideB);
        }

        public static double SelectAngleInDoc(string message, Point2d basePoint)
        {
            return SelectAngleInDoc(message, new Point3d(basePoint.X, basePoint.Y, 0));
        }

        public static double SelectAngleInDoc(string message, Point3d basePoint)
        {
            (_, Editor AcEditor) = GetCurrentDocSpace();

            var pao = new PromptAngleOptions(message)
            {
                AllowArbitraryInput = true,
                AllowNone = true,
                AllowZero = true,
                BasePoint = basePoint,
                UseBasePoint = true,
                UseDashedLine = true,
                DefaultValue = 0,
                UseDefaultValue = true
            };
            PromptDoubleResult ar = AcEditor.GetAngle(pao);
            if (ar.Status == PromptStatus.Cancel)
            {
                return -1;
            }

            return (180 / Math.PI) * ar.Value;
        }

        public static void AddObjectToDrawing(Entity entity)
        {
            (Document acDocument, Editor acEditor) = GetCurrentDocSpace();
            Database acDatabase = acDocument.Database;

            using (Transaction acTrans = acDatabase.TransactionManager.StartTransaction())
            {
                // Open the Block table record for read
                var acBlkTbl = acTrans.GetObject(acDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                entity.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(entity);
                acTrans.AddNewlyCreatedDBObject(entity, true);
                acTrans.Commit();
            }
        }

        public static void AddPointToDrawing(Point3d point, string blockTableRecordSpace)
        {
            (Document AcDocument, _) = GetCurrentDocSpace();
            Database AcDatabase = AcDocument.Database;
            using (Transaction trans = AcDatabase.TransactionManager.StartTransaction())
            {
                // Open the Block table record for read
                var acBlkTbl = trans.GetObject(AcDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                var acBlkTblRec = trans.GetObject(acBlkTbl[blockTableRecordSpace], OpenMode.ForWrite) as BlockTableRecord;

                DBPoint pointObj = new DBPoint(point);
                pointObj.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(pointObj);
                trans.AddNewlyCreatedDBObject(pointObj, true);
                trans.Commit();
            }
        }
        #endregion

        #region Private Methods


        /// <summary>
        /// Get the current document and editor
        /// </summary>
        /// <returns>Document and Editor</returns>
        public static (Document AcDocument, Editor AcEditor) GetCurrentDocSpace()
        {
            Document document = AcApplication.DocumentManager.MdiActiveDocument;
            return (document, document.Editor);
        }

        //TODO: Add more reliability 
        /// <summary>
        /// Verify the ZenethSnap (OSnapZ) is set to the preferred setting.
        /// </summary>
        /// <param name="preferredValue">false - disabled, true - enabled</param>
        /// <returns>true if match or override, false if cancel.</returns>
        private static bool VerifyZenthValues(bool preferredValue)
        {
            (_, Editor AcEditor) = GetCurrentDocSpace();
            bool OSnapZ = Convert.ToBoolean(AcApplication.TryGetSystemVariable("OSnapZ")); //0 [false] - Disabled / 1 [true] - enabled

            // If the preferred value is equal to the set value, return true (passed) or return true (passed) if not required.
            if (preferredValue == OSnapZ || !RequireZenethCheck)
            {
                RequireZenethCheck = false;
                return true;
            }

            //FEATURE: Enable temporary disablement of AutoCAD variables.
            var pkwo = new PromptKeywordOptions($"OSnapZ is {(preferredValue ? "disabled" : "enabled")}, Do you want to continue?");
            pkwo.Keywords.Add("Yes");
            pkwo.Keywords.Add("No");
            pkwo.Keywords.Default = "Yes";

            //Ask for user input.
            PromptResult KeywordResult = AcEditor.GetKeywords(pkwo);
            switch (KeywordResult.StringResult)
            {
                case "No":
                    RequireZenethCheck = false;
                    return false;
                default:
                    RequireZenethCheck = false;
                    return true;
            }
        }

        #endregion
    }
}
