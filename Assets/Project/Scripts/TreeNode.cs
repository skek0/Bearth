using System.Collections.Generic;
using UnityEngine;

// 제네릭 트리 노드 클래스
public class TreeNode<T>
{
    public T Data { get; private set; }
    public TreeNode<T> Parent { get; private set; }
    public List<TreeNode<T>> Children { get; private set; } = new List<TreeNode<T>>();

    public TreeNode(T data)
    {
        Data = data;
    }

    // 자식 추가
    public void AddChild(TreeNode<T> child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    // 자식 제거
    public void RemoveChild(TreeNode<T> child)
    {
        if (Children.Remove(child))
        {
            child.Parent = null;
        }
    }

    // 재귀적으로 트리 출력 (디버깅용)
    public void PrintTree(string indent = "")
    {
        Debug.Log($"{indent}- {Data}");
        foreach (var child in Children)
        {
            child.PrintTree(indent + "  ");
        }
    }
}