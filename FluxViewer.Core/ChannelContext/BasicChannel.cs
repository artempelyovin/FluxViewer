﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluxViewer.Core.ChannelContext;

/// <summary>
/// Класс реализующий базовый тип канала.
/// Формула имеет вид Y = XValue * X + FreeValue
/// </summary>
[Serializable]
public class BasicChannel : Channel
{
    public override string Type => "Базовый";
    
    private Double _xValue;

    public Double XValue
    {
        get { return _xValue; }
        set
        {
            this._xValue = value;
            ChannelContextHolder.Syncronize(this);
        }
    }

    private Double _freeValue;

    public Double FreeValue
    {
        get { return _freeValue; }
        set
        {
            this._freeValue = value;
            ChannelContextHolder.Syncronize(this);
        }
    }

    private int _physicalChannel;

    public int PhysicalChannel
    {
        get { return _physicalChannel; }
        set
        {
            this._physicalChannel = value;
            ChannelContextHolder.Syncronize(this);
        }
    }

    public BasicChannel() : base()
    {
        this.XValue = 1;
        this.FreeValue = 0;
        this.PhysicalChannel = 1;
    }

    public BasicChannel(int id, String name, String units, bool display, bool save, Double xValue, Double freeValue,
        int physicalChannel)
        : base(id, name, units, display, save)
    {
        this.XValue = xValue;
        this.FreeValue = freeValue;
        this.PhysicalChannel = physicalChannel;
    }
}