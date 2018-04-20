using EntityID = System.UInt16;

namespace ECS
{
    public interface IEntityAllocator
    {
         EntityID Allocate();
		 void Free(EntityID entity);
    }
}