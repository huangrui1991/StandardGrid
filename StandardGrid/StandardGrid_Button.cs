using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace StandardGrid
{
    public class StandardGrid_Button : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public StandardGrid_Button()
        {
        }

        protected override void OnClick()
        {
            //
            //  TODO: Sample code showing how to access button host
            //
            ArcMap.Application.CurrentTool = null;
            MainForm form = new MainForm();
            form.Show();
        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
