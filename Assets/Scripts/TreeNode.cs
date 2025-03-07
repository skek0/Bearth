using System.Collections.Generic;
using UnityEngine;

// ���׸� Ʈ�� ��� Ŭ����
public class TreeNode<T>
{
    public T Data { get; private set; }
    public TreeNode<T> Parent { get; private set; }
    public List<TreeNode<T>> Children { get; private set; } = new List<TreeNode<T>>();

    public TreeNode(T data)
    {
        Data = data;
    }

    // �ڽ� �߰�
    public void AddChild(TreeNode<T> child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    // �ڽ� ����
    public void RemoveChild(TreeNode<T> child)
    {
        if (Children.Remove(child))
        {
            child.Parent = null;
        }
    }

    // ��������� Ʈ�� ��� (������)
    public void PrintTree(string indent = "")
    {
        Debug.Log($"{indent}- {Data}");
        foreach (var child in Children)
        {
            child.PrintTree(indent + "  ");
        }
    }
}