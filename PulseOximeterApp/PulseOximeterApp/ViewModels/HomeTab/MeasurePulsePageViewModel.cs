﻿using Microcharts;
using PulseOximeterApp.Models;
using PulseOximeterApp.Models.HeartRate;
using PulseOximeterApp.Services;
using PulseOximeterApp.Services.BluetoothLE;
using PulseOximeterApp.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PulseOximeterApp.ViewModels.HomeTab
{
    internal class MeasurePulsePageViewModel : BaseViewModel
    {
        #region Fields
        private readonly IPulseService _pulseService;
        private readonly IList<double> _cardioIntervals;
        private readonly IList<ChartEntry> _chartEntries;
        private LineChart _lineChart;

        private readonly int _pulseBufferSize;
        private readonly int _numberOfMeasure;
        private int _valueOfCounter;
        private bool _isCompleteMeasure;

        private BaevskyIndicators _baevskyIndicators;
        #endregion

        #region Properties
        public LineChart MainChart
        {
            get => _lineChart;
            set => Set(ref _lineChart, value);
        }

        public int CounterValue
        {
            get => _valueOfCounter;
            set => Set(ref _valueOfCounter, value);
        }

        public bool IsCompleteMeasure
        {
            get => _isCompleteMeasure;
            set => Set(ref _isCompleteMeasure, value); 
        }

        public BaevskyIndicators Baevsky
        {
            get => _baevskyIndicators;
            set => Set(ref _baevskyIndicators, value);
        }
        #endregion

        #region Events
        public event Action<bool> Closing;
        #endregion

        #region Commands
        public ICommand HeadBack { get; private set; }

        private void ExecuteHeadBack(object obj)
        {
            Closing.Invoke(false);
        }

        public ICommand SaveBack { get; private set; }

        private void ExecuteSaveBack(object obj)
        {
            Closing.Invoke(true);
        }
        #endregion

        #region Base Methods
        public override void OnAppearing()
        {
            base.OnAppearing();

            (Application.Current.MainPage.BindingContext as MainPageViewModel).StatisticTabIsEnabled = false;
            (Application.Current.MainPage.BindingContext as MainPageViewModel).SettingTabIsEnabled = false;

            _pulseService.PulseNotify += OnPulseNotify;
            _pulseService.StartMeasurePulse();
        }

        public override void OnDisappearing()
        {
            if (!IsCompleteMeasure) _pulseService.StopMeasurePulse();
            _pulseService.PulseNotify -= OnPulseNotify;

            (Application.Current.MainPage.BindingContext as MainPageViewModel).StatisticTabIsEnabled = true;
            (Application.Current.MainPage.BindingContext as MainPageViewModel).SettingTabIsEnabled = true;

            base.OnDisappearing();
        }
        #endregion

        public MeasurePulsePageViewModel(IPulseService pulseService)
        {
            _pulseService = pulseService;
            _cardioIntervals = new List<double>();
            _chartEntries = new List<ChartEntry>();
            _pulseBufferSize = 4;
            _numberOfMeasure = Preferences.Get("NumberOfPulseMeasure", 30);
            _valueOfCounter = _numberOfMeasure;

            HeadBack = new Command(ExecuteHeadBack);
            SaveBack = new Command(ExecuteSaveBack);
        }

        private void OnPulseNotify(int value)
        {
            if (CounterValue > 0)
            {
                CounterValue -= 1;
                _cardioIntervals.Add(value / 1000d);

                if (_chartEntries.Count <= 30 && _cardioIntervals.Count % _pulseBufferSize == 0)
                {
                    int beatPerMinute = Convert.ToInt32(_cardioIntervals.ToList().GetRange(_cardioIntervals.Count - _pulseBufferSize, _pulseBufferSize).Sum(v => 60 / v) / _pulseBufferSize);
                    _chartEntries.Add(new ChartEntry(beatPerMinute)
                    {
                        Label = "BPM",
                        ValueLabel = beatPerMinute.ToString(),
                        Color = ChartEntryColorConverter.FromPulse(beatPerMinute),
                    });
                }
                

                if (CounterValue == 0)
                {
                    _pulseService.StopMeasurePulse();
                    MainChart = new LineChart()
                    {
                        Entries = _chartEntries
                    };

                    Baevsky = new BaevskyIndicators(new HeartRateVariability(_cardioIntervals));
                    IsCompleteMeasure = true;
                }
            }
        }
    }
}
