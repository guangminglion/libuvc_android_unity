//#define USE_OPENCV

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if USE_OPENCV
using OpenCVForUnity;
#endif

using Rect = UnityEngine.Rect;


namespace Codeflow.UvcCamera
{
    [RequireComponent(typeof(UVCCameraActivityInterface))]
    public class UVCCameraActivityInterfaceGUI : MonoBehaviour
    {
        public UVCCameraPixelFormat PixelFormat = UVCCameraPixelFormat.PIXEL_FORMAT_RGBX;
        public int Width = 1280;
        public int Height = 720;

        public Renderer TargetRenderer1;

        protected UVCCameraActivityInterface _interface;
        protected List<string> _devices = new List<string>();
        protected Texture2D _outputTexture = null;

#if USE_OPENCV
        protected Mat _rgbMat;
#endif

        protected byte[] _byteArray = null;

#if USE_OPENCV
        protected bool _doConversionToMat = false;
#endif

        protected bool _doTextureConversion = false;

        // Use this for initialization
        void Start()
        {
            _interface = GetComponent<UVCCameraActivityInterface>();

            Query();
            InvokeRepeating("UpdateOutputTexture", 0.1f, 0.1f);
        }

        void Query()
        {
            _devices.Clear();
            var deviceList = _interface.GetDeviceList();

            if(deviceList != null )
            {
                for (int i = 0; i < deviceList.Length; i++)
                {
                    _devices.Add(deviceList[i]);
                }
            }

        }

        void UpdateOutputTexture()
        {
            bool isPreviewing = _interface.GetIsPreviewing();

            if(isPreviewing)
            {
                var bufferPointer = _interface.GetBufferPtr();

                if (bufferPointer != System.IntPtr.Zero)
                {
#if USE_OPENCV
                    if (_doConversionToMat)
                    {
                        if (_rgbMat == null)
                        {
                            _rgbMat = new Mat(new Size(Width, Height), CvType.CV_8UC4);
                        }

                        OpenCVForUnity.Utils.copyToMat(bufferPointer, _rgbMat);
                    }
#endif

                    if (_doTextureConversion)
                    {
                        if (_outputTexture == null)
                        {
                            _outputTexture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                            TargetRenderer1.material.mainTexture = _outputTexture;
                        }

                        _outputTexture.LoadRawTextureData(bufferPointer, Width * Height * 4);
                        _outputTexture.Apply();
                    }
                }
            }
        }

        // Update is called once per frame
        void OnGUI()
        {
            int offset = 10;
            if (GUI.Button(new Rect(10, offset, 200, 50), "Query"))
            {
                Query();
            }
            offset += 60;

            for (int i = 0; i < _devices.Count; i++)
            {
                if (GUI.Button(new Rect(10, offset, 200, 50), _devices[i]))
                {
                    _interface.Connect(
                        deviceName: _devices[i]);
                }

                offset += 60;
            }

#if USE_OPENCV
            if (!_doConversionToMat)
            {
                if (GUI.Button(new Rect(10, offset, 200, 50), "Enable Mat Conv"))
                {
                    _doConversionToMat = true;
                }
                offset += 60;
            }
            else
            {
                if (GUI.Button(new Rect(10, offset, 200, 50), "Disable Mat Conv"))
                {
                    _doConversionToMat = false;
                }
                offset += 60;
            }
#endif

            if (!_doTextureConversion)
            {
                if (GUI.Button(new Rect(10, offset, 200, 50), "Enable Tex Conv"))
                {
                    _doTextureConversion = true;
                }
                offset += 60;
            }
            else
            {
                if (GUI.Button(new Rect(10, offset, 200, 50), "Disable Tex Conv"))
                {
                    _doTextureConversion = false;
                }
                offset += 60;
            }

            bool isConnected = _interface.GetIsConnected();

            if (isConnected)
            {
                if (GUI.Button(new Rect(10, offset, 200, 50), "Disconnect"))
                {
                    _interface.Disconnect();
                }
                offset += 60;

                bool isPreviewing = _interface.GetIsPreviewing();
                bool changed = false;

                if (GUI.Button(new Rect(10, offset, 200, 50), "StartPreview"))
                {
                    _interface.StartPreview(Width, Height, PixelFormat);
                    changed = true;
                }
                offset += 60;

                int offset2 = 10;

                if (GUI.Button(new Rect(220, offset2, 200, 50), UVCCameraPixelFormat.PIXEL_FORMAT_NV21.ToString()))
                {
                    PixelFormat = UVCCameraPixelFormat.PIXEL_FORMAT_NV21;
                    changed = true;
                }
                offset2 += 60;

                if (GUI.Button(new Rect(220, offset2, 200, 50), UVCCameraPixelFormat.PIXEL_FORMAT_RAW.ToString()))
                {
                    PixelFormat = UVCCameraPixelFormat.PIXEL_FORMAT_RAW;
                    changed = true;
                }
                offset2 += 60;

                if (GUI.Button(new Rect(220, offset2, 200, 50), UVCCameraPixelFormat.PIXEL_FORMAT_RGB565.ToString()))
                {
                    PixelFormat = UVCCameraPixelFormat.PIXEL_FORMAT_RGB565;
                    changed = true;
                }
                offset2 += 60;

                if (GUI.Button(new Rect(220, offset2, 200, 50), UVCCameraPixelFormat.PIXEL_FORMAT_RGBX.ToString()))
                {
                    PixelFormat = UVCCameraPixelFormat.PIXEL_FORMAT_RGBX;
                    changed = true;
                }
                offset2 += 60;

                if (GUI.Button(new Rect(220, offset2, 200, 50), UVCCameraPixelFormat.PIXEL_FORMAT_YUV.ToString()))
                {
                    PixelFormat = UVCCameraPixelFormat.PIXEL_FORMAT_YUV;
                    changed = true;
                }
                offset2 += 60;

                if (GUI.Button(new Rect(220, offset2, 200, 50), UVCCameraPixelFormat.PIXEL_FORMAT_YUV420SP.ToString()))
                {
                    PixelFormat = UVCCameraPixelFormat.PIXEL_FORMAT_YUV420SP;
                    changed = true;
                }
                offset2 += 60;

                //
                if (isPreviewing && changed)
                {
                    _interface.ChangePreviewFormat(PixelFormat);
                }

                if (!isPreviewing)
                { 
                    // resolutions  
                    var resolutions = _interface.GetSupportedResolutions();
                    int offset3 = 10;

                    if (resolutions != null)
                    {
                        foreach (var resolution in resolutions)
                        {
                            if (GUI.Button(new Rect(430, offset3, 200, 50), resolution))
                            {
                                string[] res = resolution.ToString().Split('x');

                                Width = int.Parse(res[0]);
                                Height = int.Parse(res[1]);

                                if (_outputTexture != null)
                                {
                                    _outputTexture = null;
                                }
                            }
                            offset3 += 60;
                        }
                    }
                }
            }
        }
    }

}
