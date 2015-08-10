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

        private IPoint leftUpPoint = new PointClass();
        private IPoint rightDownPoint = new PointClass();

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

        public IPoint LeftUpPoint
        {
            get
            {
                return leftUpPoint;
            }
            set
            {
                leftUpPoint = value;
            }
        }

        public IPoint RightDownPoint
        {
            get
            {
                return rightDownPoint;
            }
            set
            {
                rightDownPoint = value;
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
                        DeltaLatitude = 0.333333;
                        break;
                    }
                case "1:50000":
                    {
                        ScaleCode = "E";
                        DeltaLongitude = 0.25;
                        DeltaLatitude = 0.166667;
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

            int LUPointRowCode = (int)(4 / DeltaLatitude - (int)((leftUpPoint.Y % 4) / DeltaLatitude));
            int LUPointColunmCode = (int)((leftUpPoint.X % 6) / DeltaLongitude) + 1;

            int RDPointRowCode = (int)(4 / DeltaLatitude - (int)((RightDownPoint.Y % 4) / DeltaLatitude));
            int RDPointColunmCode = (int)((RightDownPoint.X % 6) / DeltaLongitude) + 1;

            //config stantard code
            string LUPointRowCode_1000000 = ((char)((int)('A') + LUPointLatitude_1000000 - 1)).ToString();
            string LUPointColunmCode_1000000 = LUPointLongtitude_1000000.ToString();
            string RDPointRowCode_1000000 = ((char)((int)('A') + RDPointLatitude_1000000 - 1)).ToString();
            string RDPointColunmCode_1000000 = RDPointLongtitude_1000000.ToString();

            int LUPointRowCode_100 = LUPointRowCode / 100;
            int LUPointRowCode_10 = (LUPointRowCode - LUPointRowCode_100 * 100)/10;
            int LUPointRowCode_1 = LUPointRowCode - LUPointRowCode_100 * 100 - LUPointRowCode_10 * 10;
            string LUPOintRowCode_str = LUPointRowCode_100.ToString() + LUPointRowCode_10.ToString() + LUPointRowCode_1.ToString();

            int LUPointColunmCode_100 = LUPointColunmCode / 100;
            int LUPointColunmCode_10 = (LUPointColunmCode - LUPointColunmCode_100 * 100) / 10;
            int LUPointColunmCode_1 = LUPointColunmCode - LUPointColunmCode_100 * 100 - LUPointColunmCode_10 * 10;
            string LUPointColunmCode_str = LUPointColunmCode_100.ToString() + LUPointColunmCode_10.ToString() + LUPointColunmCode_1.ToString();

            int RDPointRowCode_100 = RDPointRowCode / 100;
            int RDPointRowCode_10 = (RDPointRowCode - RDPointRowCode_100 * 100)/10;
            int RDPointRowCode_1 = RDPointRowCode - RDPointRowCode_100 * 100 - RDPointRowCode_10 * 10;
            string RDPOintRowCode_str = RDPointRowCode_100.ToString() + RDPointRowCode_10.ToString() + RDPointRowCode_1.ToString();

            int RDPointColunmCode_100 = RDPointColunmCode / 100;
            int RDPointColunmCode_10 = (RDPointColunmCode - RDPointColunmCode_100 * 100) / 10;
            int RDPointColunmCode_1 = RDPointColunmCode - RDPointColunmCode_100 * 100 - RDPointColunmCode_10 * 10;
            string RDPointColunmCode_str = RDPointColunmCode_100.ToString() + RDPointColunmCode_10.ToString() + RDPointColunmCode_1.ToString();

            leftUpPointUintCode = (LUPointRowCode_1000000 + LUPointColunmCode_1000000 + ScaleCode + LUPOintRowCode_str + LUPointColunmCode_str).Replace("-", "");
            rightDownPointUnitCode = (RDPointRowCode_1000000 + RDPointColunmCode_1000000 + ScaleCode + RDPOintRowCode_str + RDPointColunmCode_str).Replace("-", "");
            //MessageBox.Show(leftUpPointUintCode + "," + rightDownPointUnitCode);

            LeftUpPoint.X = (LUPointLongtitude_1000000 - 31) * 6 + (LUPointColunmCode - 1) * DeltaLongitude;
            LeftUpPoint.Y = (LUPointLatitude_1000000 - 1) * 4 + (4 / DeltaLatitude - LUPointRowCode) * DeltaLatitude;
            RightDownPoint.X = (RDPointLongtitude_1000000 - 31) * 6 + (RDPointColunmCode - 1) * DeltaLongitude;
            RightDownPoint.Y = (RDPointLatitude_1000000 - 1) * 4 + (4 / DeltaLatitude - RDPointRowCode) * DeltaLatitude;

            //MessageBox.Show(LeftUpPoint.X.ToString() +" "+LeftUpPoint.Y.ToString() + " " + RightDownPoint.X.ToString() + " " + RightDownPoint.Y.ToString());
        }

        //create standard grid map
        private void _CreateFeature()
        {
            string LUPointUnitCode_100W = leftUpPointUintCode.Substring(0, 3);
            string RDPointUnitCode_100W = rightDownPointUnitCode.Substring(0, 3);
            string LUScale = leftUpPointUintCode.Substring(3, 1);
            string RDScale = rightDownPointUnitCode.Substring(3, 1);
            int LURow = Convert.ToInt32(leftUpPointUintCode.Substring(4, 3));
            int LUColunm = Convert.ToInt32(leftUpPointUintCode.Substring(7,3));
            int RDRow = Convert.ToInt32(rightDownPointUnitCode.Substring(4, 3));
            int RDColunm = Convert.ToInt32(rightDownPointUnitCode.Substring(7, 3));

            if (LUScale != RDScale)
                return;
            if(LUPointUnitCode_100W != RDPointUnitCode_100W)
                return;

            IWorkspaceFactory wspf = new ShapeFileWorkspaceFactory();
        }

        #endregion

        public void Create()
        {
            _GetMapExtent();
        }



    }
}
