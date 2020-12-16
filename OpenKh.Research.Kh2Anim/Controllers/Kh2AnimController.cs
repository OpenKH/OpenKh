using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Research.Kh2Anim.Models;
using OpenKh.Research.Kh2Anim.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

// See also: https://docs.microsoft.com/aspnet/core/web-api/

namespace OpenKh.Research.Kh2Anim.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class Kh2AnimController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<BuildMatricesResponse> BuildMatrices(BuildMatricesRequest req)
        {
            var anbIndir = new AnbIndir(
                new Bar.Entry[]
                {
                    new Bar.Entry
                    {
                        Name = "A000",
                        Type = Bar.EntryType.Motion,
                        Stream = new MemoryStream(req.Motion),
                    }
                }
            );

            var provider = anbIndir.GetAnimProvider(
                new MemoryStream(req.Mdlx, false)
            );

            double previousGameTime = 0;

            return Ok(
                new BuildMatricesResponse
                {
                    MatrixCount = provider.MatrixCount,
                    Matrices = req.KeyFrames
                        .Select(
                            newGameTime =>
                            {
                                var delta = newGameTime - previousGameTime;
                                var matrixArray = provider.ProvideMatrices(delta);
                                previousGameTime = newGameTime;

                                return MatrixArrayToFloatsArray(matrixArray)
                                    .ToArray();
                            }
                        )
                        .ToArray(),
                }
            );
        }

        private IEnumerable<float[]> MatrixArrayToFloatsArray(Matrix4x4[] matrixArray)
        {
            foreach (var matrix in matrixArray)
            {
                yield return new float[] {
                    matrix.M11,
                    matrix.M12,
                    matrix.M13,
                    matrix.M14,
                    matrix.M21,
                    matrix.M22,
                    matrix.M23,
                    matrix.M24,
                    matrix.M31,
                    matrix.M32,
                    matrix.M33,
                    matrix.M34,
                    matrix.M41,
                    matrix.M42,
                    matrix.M43,
                    matrix.M44,
                };
            }
        }
    }
}
