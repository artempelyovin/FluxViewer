using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluxViewer.Core.ChannelContext
{
    /// <summary>
    /// Класс отдельного канала
    /// </summary>

    [Serializable]
    public class Channel
    {
        private int _id;
        public int Id 
        {
            get
            {
                return _id;
            }
            set
            {
                this._id = value;
                ChannelContextHolder.Syncronize(this);
            }
        }

        private String _name;
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                this._name = value;
                ChannelContextHolder.Syncronize(this);
            }
        }

        private String _units;
        public String Units
        {
            get
            {
                return _units;
            }
            set
            {
                this._units = value;
                ChannelContextHolder.Syncronize(this);
            }
        }

        private bool _display;
        public bool Display
        {
            get
            {
                return _display;
            }
            set
            {
                this._display = value;
                ChannelContextHolder.Syncronize(this);
            }
        }

        public bool _save;
        public bool Save
        {
            get
            {
                return _save;
            }
            set
            {
                this._save = value;
                ChannelContextHolder.Syncronize(this);
            }
        }

        /// <summary>
        /// Тип канала
        /// </summary>
        private IChannelType _channelType;
        public IChannelType ChannelType
        {
            get
            {
                return _channelType;
            }
            set
            {
                this._channelType = value;
                ChannelContextHolder.Syncronize(this);
            }
        }

        public Channel()
        {
            this.Id = 0;
            this.Name = String.Format("Канал{0:d}", this.Id);
            this.Units = "усл. ед.";
            this.Display = true;
            this.Save = true;
            this.ChannelType = new BasicType();
        }

        public Channel(int id, String name, String units, bool display, bool save, IChannelType channelType)
        {
            this.Id = id;
            this.Name = name;
            this.Units = units;
            this.Display = display;
            this.Save = save;
            this.ChannelType = channelType;
        }

        override
        public String ToString()
        {
            return $"{{\"id\" : \"{this.Id}\", \"name\" : \"{this.Name}\", \"units\" : \"{this.Units}\", \"display\" : \"{this.Display}\", " +
                $"\"save\" : \"{this.Save}\", \"channelType\" : {this.ChannelType.ToString()}}}";
        }

    }
}
