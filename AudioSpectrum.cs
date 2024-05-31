using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using STOP_MODE = FMOD.Studio.STOP_MODE;

/// <summary>
/// Мини "движок" для анализа данных музыкального спектра
/// </summary>
public class AudioSpectrum : MonoBehaviour
{
    [SerializeField] private int windowSize = 32;
    [SerializeField] private DSP_FFT_WINDOW windowShape = DSP_FFT_WINDOW.HAMMING;

    private string _eventPath;
    private EventInstance _event;
    private ChannelGroup _channelGroup;
    private DSP _dsp;
    private DSP_PARAMETER_FFT _fftParam;
    
    public float spectrumValue {get; private set;}
    
    private float[] m_audioSpectrum;

    private void Awake()
    {
        m_audioSpectrum = new float[windowSize];
    }

    private void Update()
    {
        GetSpectrumData();
        
        if (m_audioSpectrum != null && m_audioSpectrum.Length > 0)
            spectrumValue = m_audioSpectrum[0] * 100;
    }
    
    public void SetFmodEventInstance(string eventPath)
    {
        _eventPath = eventPath;
        _event = FMODAudioManager.Instance.GetEvent(eventPath);
    }

    public void UpdatePositionForEvent(Transform objectToPlayEventOn)
    {
        if (objectToPlayEventOn && _event.isValid())
            _event.set3DAttributes(objectToPlayEventOn.To3DAttributes());
    }

    public void PlayEvent()
    {
        _event.start();
        PrepareFmodEventInstance();
    }

    public void PauseEvent(bool isPaused)
    {
        _event.setPaused(isPaused);
    }

    public void StopEvent()
    {
        _event.stop(STOP_MODE.ALLOWFADEOUT);
        
        if(!string.IsNullOrEmpty(_eventPath))
            FMODAudioManager.Instance.ReleaseEvent(_eventPath);
        
        _eventPath = null;
    }

    public void SetTapeVolume(float volume)
    {
        _event.setVolume(volume);
    }
    
    public int TimelinePosition
    {
        get
        {
            _event.getTimelinePosition(out var value);
            return value;
        }

        set
        {
            _event.setTimelinePosition(value);
        }
    }
    
    public string GetSoundName()
    {
        if (string.IsNullOrEmpty(_eventPath)) return null;
        List<string> splittedEventPath = new List<string>(_eventPath.Split('/').Select(path => path).ToList());
        return splittedEventPath.LastOrDefault();
    }

    public void PrepareFmodEventInstance()
    { 
        RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.FFT, out _dsp);
        _dsp.setParameterInt((int)DSP_FFT.WINDOWTYPE, (int)windowShape);
        _dsp.setParameterInt((int)DSP_FFT.WINDOWSIZE, windowSize * 2);

        _event.getChannelGroup(out _channelGroup);
        _channelGroup.addDSP(0, _dsp);
    }

    private void GetSpectrumData()
    {
        if (_dsp.getParameterData((int)DSP_FFT.SPECTRUMDATA, out var data, out var length) == RESULT.OK)
        {
            
            _fftParam = (DSP_PARAMETER_FFT)Marshal.PtrToStructure(data, typeof(DSP_PARAMETER_FFT));
            
            if (_fftParam.numchannels == 0)
            {
                if(_event.getChannelGroup(out _channelGroup) == RESULT.OK)
                    _channelGroup.addDSP(0, _dsp);
            }
            else if (_fftParam.numchannels >= 1)
            {
                for (int s = 0; s < windowSize; s++)
                {
                    float totalChannelData = 0f;

                    for (int c = 0; c < _fftParam.numchannels; c++)
                        totalChannelData += _fftParam.spectrum[c][s];

                    m_audioSpectrum[s] = totalChannelData / _fftParam.numchannels;
                }
            }
        }
    }
}
