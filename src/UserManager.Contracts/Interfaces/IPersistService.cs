using UserManager.Contracts.DTO;

namespace UserManager.Contracts.Interfaces;
public interface IPersistService
{
    T Get<T>(string key);
    void Set<T>(string key, T value);
    void Remove(string key);
}