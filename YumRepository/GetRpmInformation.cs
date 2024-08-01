namespace ArxOne.Yum;

public delegate (IReadOnlyDictionary<string, object?> Signature, IReadOnlyDictionary<string, object?> Header) GetRpmInformation(string path);