using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
namespace MisFrameWork.core.converter
{
	public interface IObjectConverter
	{
        object FromString(object str);
        string ToString(object obj);
	}
}
