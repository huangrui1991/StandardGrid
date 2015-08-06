using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using System.Windows.Forms;

namespace StandardGrid
{
    public class GridCreater
    {
        private string imagePath;
        private string targetFolder;
        private int spatialReference;

        #region Properties
        public string ImagePath
        {
            set
            {
                imagePath = value;
            }
            get
            {
                return imagePath;
            }
        }

        public string TargetFolder
        {
            set
            {
                targetFolder = value;
            }
            get
            {
                return targetFolder;
            }
        }

        public int SpatialReference
        {
            get
            {
                return spatialReference;
            }
        }
        #endregion

        #region private function
        private void _GetSpatialReference()
        {
            System.IO.FileInfo Info = new System.IO.FileInfo(imagePath);
            string ImageFolder = Info.Directory.FullName;
            string ImageName = Info.Name;

            if (Info.Exists)
            {
                IWorkspaceFactory WorkspaceFactory = new RasterWorkspaceFactoryClass();
                IRasterWorkspace Workspace = WorkspaceFactory.OpenFromFile(ImageFolder,0) as IRasterWorkspace;
                IGeoDataset Dataset = Workspace.OpenRasterDataset(ImageName) as IGeoDataset;
                spatialReference = Dataset.SpatialReference.FactoryCode;
                MessageBox.Show(spatialReference.ToString());
            }
            else
            {
                MessageBox.Show("Image file path error!");
            }
            
        }

        private void _GetMapExtent()
        {
        }

        private void _GetMapUnitCode()
        {
        }

        private void _GenerateFishNet()
        {
        }
        #endregion

        public bool Create()
        {
            _GetSpatialReference();
            return true;
        }




    }
}
