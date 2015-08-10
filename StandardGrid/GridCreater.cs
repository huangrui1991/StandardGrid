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
        private string leftDownPointUintCode;
        private string rightUpPointUnitCode;

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
                    IPoint LeftDownPoint = new PointClass();
                    IPoint RightUpPoint = new PointClass();
                    LeftDownPoint.X = Envelope.XMin;
                    LeftDownPoint.Y = Envelope.YMin;
                    RightUpPoint.X = Envelope.XMax;
                    RightUpPoint.Y = Envelope.YMax;
                    if (spatialReference == (int)esriSRGeoCSType.esriSRGeoCS_WGS1984)
                    {
                        _GetMapUnitCode(LeftDownPoint, RightUpPoint);
                    }
                    else /*if (EsriSRProjCSTypes.Contains(spatialReference))*/
                    {
                        ISpatialReferenceFactory SRF = new SpatialReferenceEnvironmentClass();
                        LeftDownPoint.SpatialReference = SRF.CreateProjectedCoordinateSystem(spatialReference);
                        RightUpPoint.SpatialReference = SRF.CreateProjectedCoordinateSystem(spatialReference);

                        LeftDownPoint.Project(SRF.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984));
                        RightUpPoint.Project(SRF.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984));

                        _GetMapUnitCode(LeftDownPoint, RightUpPoint);
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
        private void _GetMapUnitCode(IPoint leftDownPoint,IPoint RightUpPoint)
        {
            //compute 1:100000 map unit code
            int LDPointLatitude_1000000 = (int)(leftDownPoint.Y / 4) + 1; 
            int LDPointLongtitude_1000000 = (int)(leftDownPoint.X / 6) + 31;
            int RUPointLatitude_1000000 = (int)(RightUpPoint.Y / 4) + 1;
            int RUPointLongtitude_1000000 = (int)(RightUpPoint.X / 6) + 31;

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

            int LDPOintRowCode = (int)(4 / DeltaLatitude - (int)((leftDownPoint.Y / 4) / DeltaLatitude));
            int LDPointColunmCode = (int)((leftDownPoint.X / 6) / DeltaLongitude) + 1;

            int RUPOintRowCode = (int)(4 / DeltaLatitude - (int)((RightUpPoint.Y / 4) / DeltaLatitude));
            int RUPointColunmCode = (int)((RightUpPoint.X / 6) / DeltaLongitude) + 1;

            //config stantard code
            string LDPointRowCode_1000000 = ((char)((int)('A') + LDPointLatitude_1000000 - 1)).ToString();
            string LDPointColunmCode_1000000 = LDPointLongtitude_1000000.ToString();
            string RUPointRowCode_1000000 = ((char)((int)('A') + RUPointLatitude_1000000 - 1)).ToString();
            string RUPointColunmCode_1000000 = RUPointLongtitude_1000000.ToString();

            int LDPointRowCode_100 = LDPOintRowCode / 100;
            int LDPointRowCode_10 = (LDPOintRowCode - LDPointRowCode_100 * 100)/10;
            int LDPointRowCode_1 = LDPOintRowCode - LDPointRowCode_100 * 100 - LDPointRowCode_10 * 10;
            string LDPOintRowCode_str = LDPointRowCode_100.ToString() + LDPointRowCode_10.ToString() + LDPointRowCode_1.ToString();

            int LDPointColunmCode_100 = LDPOintRowCode / 100;
            int LDPointColunmCode_10 = (LDPOintRowCode - LDPointColunmCode_100 * 100)/10;
            int LDPointColunmCode_1 = LDPOintRowCode - LDPointColunmCode_100 * 100 - LDPointColunmCode_10 * 10;
            string LDPointColunmCode_str = LDPointColunmCode_100.ToString() + LDPointColunmCode_10.ToString() + LDPointColunmCode_1.ToString();

            int RUPointRowCode_100 = RUPOintRowCode / 100;
            int RUPointRowCode_10 = (RUPOintRowCode - RUPointRowCode_100 * 100)/10;
            int RUPointRowCode_1 = RUPOintRowCode - RUPointRowCode_100 * 100 - RUPointRowCode_10 * 10;
            string RUPOintRowCode_str = RUPointRowCode_100.ToString() + RUPointRowCode_10.ToString() + RUPointRowCode_1.ToString();

            int RUPointColunmCode_100 = RUPOintRowCode / 100;
            int RUPointColunmCode_10 = (RUPOintRowCode - RUPointColunmCode_100 * 100)/10;
            int RUPointColunmCode_1 = RUPOintRowCode - RUPointColunmCode_100 * 100 - RUPointColunmCode_10 * 10;
            string RUPointColunmCode_str = RUPointColunmCode_100.ToString() + RUPointColunmCode_10.ToString() + RUPointColunmCode_1.ToString();

            leftDownPointUintCode = LDPointRowCode_1000000 + LDPointColunmCode_1000000 + ScaleCode + LDPOintRowCode_str + LDPointColunmCode_str;
            rightUpPointUnitCode = RUPointRowCode_1000000 + RUPointColunmCode_1000000 + ScaleCode + RUPOintRowCode_str + RUPointColunmCode_str;
            MessageBox.Show(leftDownPointUintCode.Replace("-", "") + "," + rightUpPointUnitCode.Replace("-", ""));

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
