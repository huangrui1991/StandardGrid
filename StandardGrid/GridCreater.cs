using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;

namespace StandardGrid
{
    public class GridCreater
    {
        private string imagePath;
        private string targetFolder;
        private int spatialReference;
        private string scale;
        private string leftUpPointUintCode;
        private string rightDownPointUnitCode;

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

        public  int SpatialReference
        {
            get
            {
                return spatialReference;
            }
        }

        public string Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }
        #endregion

        #region private function
        private void _GetMapExtent()
        {
            System.IO.FileInfo Info = new System.IO.FileInfo(imagePath);
            string ImageFolder = Info.Directory.FullName;
            string ImageName = Info.Name;

            if (Info.Exists)
            {
                IWorkspaceFactory WorkspaceFactory = new RasterWorkspaceFactoryClass();
                IRasterWorkspace Workspace = WorkspaceFactory.OpenFromFile(ImageFolder,0) as IRasterWorkspace;
                IGeoDataset Dataset = Workspace.OpenRasterDataset(ImageName) as IGeoDataset;
                //get spatial reference
                spatialReference = Dataset.SpatialReference.FactoryCode;
                
                //get all Project Coordinate system
                List<int> EsriSRProjCSTypes = new List<int>();
                foreach(int type in Enum.GetValues((typeof(esriSRProjCSType))))
                {
                    EsriSRProjCSTypes.Add(type);
                }
                
                //get extent transform spatial reference to GCS
                if (spatialReference != 0)
                {

                    MessageBox.Show(spatialReference.ToString());
                    IEnvelope Envelope = Dataset.Extent;
                    IPoint LeftUpPoint = new PointClass();
                    IPoint RightDownPoint = new PointClass();
                    LeftUpPoint.X = Envelope.XMin;
                    LeftUpPoint.Y = Envelope.YMin;
                    RightDownPoint.X = Envelope.XMax;
                    RightDownPoint.Y = Envelope.YMax;
                    if (spatialReference == (int)esriSRGeoCSType.esriSRGeoCS_WGS1984)
                    {
                        _GetMapUnitCode(LeftUpPoint, RightDownPoint);
                    }
                    else /*if (EsriSRProjCSTypes.Contains(spatialReference))*/
                    {
                        ISpatialReferenceFactory SRF = new SpatialReferenceEnvironmentClass();
                        LeftUpPoint.SpatialReference = SRF.CreateProjectedCoordinateSystem(spatialReference);
                        RightDownPoint.SpatialReference = SRF.CreateProjectedCoordinateSystem(spatialReference);

                        LeftUpPoint.Project(SRF.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984));
                        RightDownPoint.Project(SRF.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984));

                        _GetMapUnitCode(LeftUpPoint, RightDownPoint);
                    }
                    

                }

            }
            else
            {
                MessageBox.Show("Image file path error!");
                return;
            }
        }


        //IPoint coordinate must be esriSRGeoCS_WGS1984
        private void _GetMapUnitCode(IPoint leftUpPoint,IPoint RightDownPoint)
        {
            //compute 1:100000 map unit code
            int LUPointLatitude_1000000 = (int)(leftUpPoint.Y / 4) + 1; 
            int LUPointLongtitude_1000000 = (int)(leftUpPoint.X / 6) + 31;
            int RDPointLatitude_1000000 = (int)(RightDownPoint.Y / 4) + 1;
            int RDPointLongtitude_1000000 = (int)(RightDownPoint.X / 6) + 31;

            //compute each scale map unit code
            double DeltaLatitude = 0;
            double DeltaLongitude = 0;
            string ScaleCode = "";

            switch (Scale)
            {
                case "1:500000":
                    {
                        ScaleCode = "B";
                        DeltaLongitude = 3;
                        DeltaLatitude = 2;
                        break;
                    }
                case "1:250000":
                    {
                        ScaleCode = "C";
                        DeltaLongitude = 1.5;
                        DeltaLatitude = 1;
                        break;
                    }
                case "1:100000":
                    {
                        ScaleCode = "D";
                        DeltaLongitude = 0.5;
                        DeltaLatitude = 0.333;
                        break;
                    }
                case "1:50000":
                    {
                        ScaleCode = "E";
                        DeltaLongitude = 0.25;
                        DeltaLatitude = 0.1667;
                        break;
                    }
                case "1:25000":
                    {
                        ScaleCode = "F";
                        DeltaLongitude = 0.125;
                        DeltaLatitude = 0.083333;
                        break;
                    }
                case "1:10000":
                    {
                        ScaleCode = "H";
                        DeltaLongitude = 0.0625;
                        DeltaLatitude = 0.042;
                        break;
                    }
                case "1:5000":
                    {
                        ScaleCode = "I";
                        DeltaLongitude = 0.03125;
                        DeltaLatitude = 0.021;
                        break;
                    }
                default:
                    return;
            }

            int LUPOintRowCode = (int)(4 / DeltaLatitude - (int)((leftUpPoint.Y / 4) / DeltaLatitude));
            int LUPointColunmCode = (int)((leftUpPoint.X / 6) / DeltaLongitude) + 1;

            int RDPOintRowCode = (int)(4 / DeltaLatitude - (int)((RightDownPoint.Y / 4) / DeltaLatitude));
            int RDPointColunmCode = (int)((RightDownPoint.X / 6) / DeltaLongitude) + 1;

            //config stantard code
            string LUPointRowCode_1000000 = ((char)((int)('A') + LUPointLatitude_1000000 - 1)).ToString();
            string LUPointColunmCode_1000000 = LUPointLongtitude_1000000.ToString();
            string RDPointRowCode_1000000 = ((char)((int)('A') + RDPointLatitude_1000000 - 1)).ToString();
            string RDPointColunmCode_1000000 = RDPointLongtitude_1000000.ToString();

            int LUPointRowCode_100 = LUPOintRowCode / 100;
            int LUPointRowCode_10 = (LUPOintRowCode - LUPointRowCode_100 * 100)/10;
            int LUPointRowCode_1 = LUPOintRowCode - LUPointRowCode_100 * 100 - LUPointRowCode_10 * 10;
            string LUPOintRowCode_str = LUPointRowCode_100.ToString() + LUPointRowCode_10.ToString() + LUPointRowCode_1.ToString();

            int LUPointColunmCode_100 = LUPOintRowCode / 100;
            int LUPointColunmCode_10 = (LUPOintRowCode - LUPointColunmCode_100 * 100)/10;
            int LUPointColunmCode_1 = LUPOintRowCode - LUPointColunmCode_100 * 100 - LUPointColunmCode_10 * 10;
            string LUPointColunmCode_str = LUPointColunmCode_100.ToString() + LUPointColunmCode_10.ToString() + LUPointColunmCode_1.ToString();

            int RDPointRowCode_100 = RDPOintRowCode / 100;
            int RDPointRowCode_10 = (RDPOintRowCode - RDPointRowCode_100 * 100)/10;
            int RDPointRowCode_1 = RDPOintRowCode - RDPointRowCode_100 * 100 - RDPointRowCode_10 * 10;
            string RDPOintRowCode_str = RDPointRowCode_100.ToString() + RDPointRowCode_10.ToString() + RDPointRowCode_1.ToString();

            int RDPointColunmCode_100 = RDPOintRowCode / 100;
            int RDPointColunmCode_10 = (RDPOintRowCode - RDPointColunmCode_100 * 100)/10;
            int RDPointColunmCode_1 = RDPOintRowCode - RDPointColunmCode_100 * 100 - RDPointColunmCode_10 * 10;
            string RDPointColunmCode_str = RDPointColunmCode_100.ToString() + RDPointColunmCode_10.ToString() + RDPointColunmCode_1.ToString();

            leftUpPointUintCode = LUPointRowCode_1000000 + LUPointColunmCode_1000000 + ScaleCode + LUPOintRowCode_str + LUPointColunmCode_str;
            rightDownPointUnitCode = RDPointRowCode_1000000 + RDPointColunmCode_1000000 + ScaleCode + RDPOintRowCode_str + RDPointColunmCode_str;
            MessageBox.Show(leftUpPointUintCode.Replace("-", "") + "," + rightDownPointUnitCode.Replace("-", ""));

        }

        //create standard grid map
        private void _CreateFeature()
        {

        }

        #endregion

        public void Create()
        {
            _GetMapExtent();
        }



    }
}
