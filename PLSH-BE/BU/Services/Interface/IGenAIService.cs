using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BU.Services.Interface
{
    public interface IGenAIService
    {
        Task<List<int>> GetBookRecommendations(string bookIds);
    }
}
