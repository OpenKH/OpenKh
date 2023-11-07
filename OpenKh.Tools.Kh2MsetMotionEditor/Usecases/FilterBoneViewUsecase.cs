using OpenKh.Tools.Kh2MsetMotionEditor.Models.BoneDictSpec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class FilterBoneViewUsecase
    {
        private readonly GetBoneDictElementUsecase _getBoneDictElementUsecase;

        public FilterBoneViewUsecase(
            GetBoneDictElementUsecase getBoneDictElementUsecase
        )
        {
            _getBoneDictElementUsecase = getBoneDictElementUsecase;
        }

        public IEnumerable<BoneElement> Filter(
            IEnumerable<string> tokens
        )
        {
            bool IsMatch(string? viewMatch)
            {
                if (string.IsNullOrEmpty(viewMatch))
                {
                    return true;
                }
                else
                {
                    var orTokens = viewMatch.Split(';')
                        .Where(it => it.Length != 0);

                    return orTokens
                        .All(
                            orToken =>
                            {
                                var andTokens = orToken.Split(',')
                                    .Where(it => it.Length != 0);

                                return andTokens.All(
                                    andToken => tokens.Any(
                                        token => string.Compare(andToken, token, true) == 0
                                    )
                                );
                            }
                        );
                }
            }

            return (_getBoneDictElementUsecase().BoneView ?? new BoneViewElement[0])
                .Where(view => IsMatch(view.Match))
                .SelectMany(
                    view => view.Bone ?? Enumerable.Empty<BoneElement>()
                )
                .GroupBy(it => it.I)
                .Select(group => group.Last())
                .ToArray();
        }
    }
}
