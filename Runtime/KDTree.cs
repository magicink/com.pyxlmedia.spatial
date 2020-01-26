using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PyxlMedia.Spatial
{
    public class KdTree<T> where T : Spatial
    {
        private readonly Func<T, float>[] _orderingDimensions;
        private readonly T _pivot;
        private readonly int _totalDimensions;
        private readonly KdTree<T> _left;
        private readonly KdTree<T> _right;

        public KdTree(ICollection<T> tree, Func<T, float>[] orderingDimensions, int depth = 0)
        {
            _orderingDimensions = orderingDimensions;
            _totalDimensions = _orderingDimensions.Length;
            var n = tree.Count;
            if (n <= 0) return;
            var dimension = depth % _totalDimensions;
            var sortedList = tree.OrderBy(_orderingDimensions[dimension]).ToList();
            var skips = (int) Math.Floor(n / 2.0);
            _pivot = sortedList.Skip(skips).Take(1).Single();

            // zero skips indicates that there is only one element, so neither left or right
            // branches are created.
            if (skips <= 0) return;
            var leftList = sortedList.Take(skips).ToList();
            _left = new KdTree<T>(leftList, _orderingDimensions, depth + 1);

            // One or more skips means that there may be a right tree.
            if (skips < 1) return;
            var rightList = sortedList.Skip(skips + 1).ToList();

            if (rightList.Count > 0) _right = new KdTree<T>(rightList, _orderingDimensions, depth + 1);
        }

        public T NearestNeighbor(T point, int depth = 0)
        {
            if (_pivot == null) return default(T);
            var dimension = depth % _totalDimensions;
            var orderingDimension = _orderingDimensions[dimension];
            var pointValue = orderingDimension(point);
            var pivotValue = orderingDimension(_pivot);
            KdTree<T> next;
            KdTree<T> opposite;
            if (pointValue < pivotValue)
            {
                next = _left;
                opposite = _right;
            }
            else
            {
                next = _right;
                opposite = _left;
            }

            var closest = ClosestPivot(point, next?.NearestNeighbor(point, depth + 1), _pivot);
            if (point.DistanceTo(closest) > Mathf.Abs(pointValue - pivotValue) && opposite != null)
                closest = ClosestPivot(point, opposite.NearestNeighbor(point, depth + 1), closest);
            return closest;
        }

        private static T ClosestPivot(T point, T p1, T p2)
        {
            if (p1 == null) return p2;
            if (p2 == null) return p1;
            var d1 = point.DistanceTo(p1);
            var d2 = point.DistanceTo(p2);
            return d1 < d2 ? p1 : p2;
        }
    }
}