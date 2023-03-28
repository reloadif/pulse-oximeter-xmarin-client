﻿using Microcharts;
using PulseOximeterApp.Services.BluetoothLE;
using PulseOximeterApp.ViewModels.Base;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace PulseOximeterApp.ViewModels.HomeTab
{
    internal class MeasurePulsePageViewModel : BaseViewModel
    {
        #region Fields
        private readonly IPulseService _pulseService;
        private readonly IList<ChartEntry> _chartEntries;
        private LineChart _lineChart;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly int _numberOfMeasure;
        private int _valueOfCounter;
        private bool _isCompleteMeasure;
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
        #endregion

        #region Base Methods
        public override void OnAppearing()
        {
            base.OnAppearing();

            (Application.Current.MainPage.BindingContext as MainPageViewModel).StatisticTabIsEnabled = false;
            (Application.Current.MainPage.BindingContext as MainPageViewModel).SettingTabIsEnabled = false;

            _pulseService.PulseNotify += OnPulseNotify;
            _cancellationTokenSource = new CancellationTokenSource();
            _pulseService.StartMeasurePulse(_cancellationTokenSource.Token);
        }

        public override void OnDisappearing()
        {
            _cancellationTokenSource.Cancel();
            _pulseService.PulseNotify -= OnPulseNotify;

            (Application.Current.MainPage.BindingContext as MainPageViewModel).StatisticTabIsEnabled = true;
            (Application.Current.MainPage.BindingContext as MainPageViewModel).SettingTabIsEnabled = true;

            base.OnDisappearing();
        }
        #endregion

        public MeasurePulsePageViewModel(IPulseService pulseService)
        {
            _pulseService = pulseService;
            _chartEntries = new List<ChartEntry>();
            _numberOfMeasure = 30;
            _valueOfCounter = _numberOfMeasure;

            HeadBack = new Command(ExecuteHeadBack);
        }

        private void OnPulseNotify(int value)
        {
            if (!_isCompleteMeasure)
            {
                CounterValue -= 1;

                _chartEntries.Add(new ChartEntry(value)
                {
                    Label = "BPM",
                    ValueLabel = value.ToString(),
                    Color = CalculateColorForChartEnty(value),
                });

                if (_chartEntries.Count > _numberOfMeasure)
                {
                    _cancellationTokenSource.Cancel();
                    IsCompleteMeasure = true;
                    MainChart = new LineChart()
                    {
                        Entries = _chartEntries
                    };
                }
            }
        }

        private SKColor CalculateColorForChartEnty(int value)
        {
            SKColor result = SKColor.Parse("f24518");

            if (value < 45) result = SKColor.Parse("f24518");
            else if (value < 80) result = SKColor.Parse("2bf518");
            else if (value < 100) result = SKColor.Parse("f1f518");

            return result;
        }
    }
}
