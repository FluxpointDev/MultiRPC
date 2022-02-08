#$1: source_dir, $2: arch, $3: package_name

arch=$2
version=`cat version-digit`
package_name=$3
package_name+="-"
package_name+=`cat version`

echo $package_name
base_dir=../packages/${package_name}
source_dir=$1

rm -r $base_dir

# Copy base files
mkdir -p $base_dir && mkdir ${base_dir}/DEBIAN
mkdir -p ${base_dir}/usr/bin/MultiRPC && cp -r $source_dir/* ${base_dir}/usr/bin/MultiRPC

# Make syslink
ln -s /usr/bin/MultiRPC/MultiRPC ${base_dir}/usr/bin/multirpc

# Copy deb stuff
cp control ${base_dir}/DEBIAN/control
sed -i s/arch-replace/${arch}/gI ${base_dir}/DEBIAN/control
sed -i s/version-replace/${version}/gI ${base_dir}/DEBIAN/control

# Create package
cd ${base_dir}
cd ../
dpkg-deb --build $package_name
