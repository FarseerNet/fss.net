workdir='/root/nfs/git/'
for dir in $(ls ${workdir});
do
    git -C $workdir$dir pull
done