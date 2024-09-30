using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluxViewer.Core.ChannelContext
{
    /// <summary>
    /// Класс реализующий перемнжающий тип
    /// </summary>

    [Serializable]
    public class MultiplicativeType : IChannelType
    {
        private List<int> _channelIds;
        public List<int> ChannelIds
        {
            get
            {
                return _channelIds;
            }
            set
            {
                this._channelIds = value;
                ChannelContextHolder.Syncronize(this);
            }
        }

        public MultiplicativeType()
        {
            this.ChannelIds = new List<int>();
        }

        public MultiplicativeType(List<int> channelIds)
        {
            this.ChannelIds = channelIds;
        }

        /// <summary>
        /// Добавляет id канала в список
        /// </summary>
        /// <param name="id">id канала</param>
        /// <exception cref="ArgumentException">Если id уже в списке</exception>
        public void AddChannelId(int id)
        {
            if (this.ChannelIds.Where(x => x == id).Count() > 0)
                throw new ArgumentException("Such Id already exists");

            this.ChannelIds.Add(id);

            ChannelContextHolder.Syncronize(this);
        }


        /// <summary>
        /// Удаляет значениt id канала из списка накалов на которые перемножается
        /// </summary>
        /// <param name="id">id канала</param>
        public void RemoveChannelId(int id)
        {
            this.ChannelIds.RemoveAll(x => x == id);

            ChannelContextHolder.Syncronize(this);
        }
    }
}
